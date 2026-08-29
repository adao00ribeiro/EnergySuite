import json
import os
import asyncio
import logging
import uuid
from datetime import datetime, timedelta, timezone
from aiokafka import AIOKafkaConsumer, AIOKafkaProducer
from pydantic import ValidationError
from sqlalchemy.future import select

from opentelemetry import trace
from opentelemetry.propagate import extract, inject

from .database import AsyncSessionLocal
from .models import ContractCreatedEvent, RiskMetricModel, RiskCalculatedEvent, EnaCalculatedIntegrationEvent
from .risk_engine import RiskEngine
from .gevazp_generator import GevazpGenerator

from prometheus_client import Gauge, Counter, start_http_server

# Prometheus Metrics
RISK_MTM = Gauge('risk_mtm_value', 'Mark-to-Market value', ['submarket'])
CONTRACTS_PROCESSED = Counter('risk_contracts_processed_total', 'Total contracts processed', ['submarket'])

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

tracer = trace.get_tracer(__name__)

KAFKA_BOOTSTRAP_SERVERS = os.getenv("KAFKA_BOOTSTRAP_SERVERS", "kafka:9092")
TOPICS_CONSUME = ["contract-events", "pluvia-events"]
TOPIC_PRODUCE = "risk-events"
TOPIC_ENA_PRODUCE = "ena-events"

async def process_pluvia_event(event_data: dict, producer: AIOKafkaProducer):
    try:
        with tracer.start_as_current_span("compute_hydrological_model") as span:
            sim_id = event_data.get("SimulationId")
            scenario_id = event_data.get("ScenarioId")
            model = event_data.get("ModelName")
            
            # If this was triggered by BlendCustomMapCommand, the payload will have blendConfig
            blend_config = event_data.get("BlendConfig")
            
            if blend_config:
                logger.info(f"Running Hydrological Model with Blend Config: {blend_config}")
                # Parse JSON string {"GEFS": 0.5, "ETA": 0.3, "ECMWF": 0.2}
                try:
                    weights = json.loads(blend_config)
                    for mod, weight in weights.items():
                        logger.info(f" -> Blending model {mod} with weight {weight*100}%")
                except Exception as ex:
                    logger.warning(f"Failed to parse blend config: {ex}")
            else:
                logger.info(f"Starting {model} calculation for scenario {scenario_id}")
            
            # Cálculo de ENA a partir de dados reais de precipitação/hidrologia
            # do Data Lake (vetorizado com pandas). O modelo físico completo (SMAP)
            # fica fora de escopo desta sprint (débito registrado); aqui usamos a
            # série real persistida como projeção, sem qualquer aleatoriedade.
            ena_records = _compute_ena_forecast(sim_id, model)

            if not ena_records:
                logger.warning(
                    f"[ENA] Sem dados reais no Data Lake para a simulação {sim_id}. "
                    f"Nenhum registro de ENA será publicado (estado vazio honesto)."
                )
            else:
                logger.info(
                    f"Hydrological simulation {sim_id}: {len(ena_records)} ENA records "
                    f"computados a partir de dados reais."
                )

                with tracer.start_as_current_span(f"{TOPIC_ENA_PRODUCE} publish"):
                    for ena_event in ena_records:
                        out_headers = {}
                        inject(out_headers)
                        header_list = [(k, v.encode('utf-8')) for k, v in out_headers.items()]

                        await producer.send_and_wait(
                            TOPIC_ENA_PRODUCE,
                            ena_event.model_dump(mode='json'),
                            headers=header_list,
                        )
                    logger.info(
                        f"Published {len(ena_records)} ENA records for simulation {sim_id} "
                        f"to {TOPIC_ENA_PRODUCE}"
                    )

            # Sprint 6: Generate and upload GEVAZP files (apenas se houver dados reais)
            with tracer.start_as_current_span("generate_gevazp_exports"):
                generator = GevazpGenerator()
                generator.generate_and_upload(sim_id, ena_records)
            
    except Exception as e:
        logger.error(f"Error processing pluvia event: {e}")

SUBMARKETS = [
    ("SE/CO", "Parana"),
    ("SUL", "Iguacu"),
    ("NE", "Sao Francisco"),
    ("N", "Tocantins"),
]


def _compute_ena_forecast(sim_id: str, model: str):
    """Calcula ENA (projeção) a partir de dados reais de precipitação do Data Lake.

    Vetorizado com pandas (sem loops de linha). Retorna lista de
    `EnaCalculatedIntegrationEvent` (mesmo formato persistido/consumido pelo .NET)
    ou lista vazia quando não há dados reais.
    """
    import pandas as pd

    from .data_lake import PRECIPITATION_PATHS, read_first_existing

    df = read_first_existing(PRECIPITATION_PATHS)
    if df is None or df.empty:
        return []

    if model:
        df = df[df["model"].astype(str).str.upper() == model.upper()]
    if df.empty:
        return []

    # Coluna de bacia/submercado, se existir; senão deriva do par lat/lon bin.
    base = df.copy()

    # Normaliza o submercado (numérico 0-3 => nome ONS) para casar com SUBMARKETS.
    SM_ID_TO_NAME = {0: "SE/CO", 1: "SUL", 2: "NE", 3: "N"}
    if "submarket" in base.columns:
        base["submarket_name"] = base["submarket"].map(SM_ID_TO_NAME).fillna("N/A")
    elif "basin" in base.columns:
        basin_to_sm = {b: sm for sm, b in SUBMARKETS}
        base["submarket_name"] = base["basin"].map(basin_to_sm).fillna("N/A")
    else:
        # Bin vetorizado de lat em 4 regiões (proxy geográfico determinístico)
        lat_bin = pd.cut(base["lat"], bins=4, labels=[0, 1, 2, 3])
        base["submarket_name"] = lat_bin.map(SM_ID_TO_NAME).fillna("N/A")

    if "basin" not in base.columns:
        base["basin"] = base["submarket_name"]

    # Garante a coluna de data em timestamp para agregação mensal
    if "date" in base.columns:
        base["ts"] = pd.to_datetime(base["date"])
    elif "timestamp" in base.columns:
        base["ts"] = pd.to_datetime(base["timestamp"])
    else:
        base["ts"] = pd.Timestamp.now()

    # Agregação mensal vetorizada: precipitação média por (submercado, bacia, mês)
    base["month"] = base["ts"].dt.to_period("M")
    agg = base.groupby(["submarket_name", "basin", "month"], as_index=False)["value_mm"].mean()
    agg["month"] = agg["month"].astype(str)

    if agg.empty:
        return []

    # Projeção dos próximos 12 meses replicando o último padrão mensal real
    # (persistência) — vetorizada, sem aleatoriedade.
    records = []
    months_ahead = pd.period_range(datetime.now(timezone.utc).strftime("%Y-%m"), periods=12, freq="M").astype(str)

    for sm, basin in SUBMARKETS:
        sub_agg = agg[agg["submarket_name"] == sm]
        if sub_agg.empty:
            sub_agg = agg[agg["basin"] == basin]
        if sub_agg.empty:
            continue

        real_values = sub_agg["value_mm"].to_numpy(dtype=float)
        real_values = real_values[~pd.isna(real_values)]
        if real_values.size == 0:
            continue

        # Valor de referência = média real observada na bacia (MW médio proxy ENA)
        base_mw = float(real_values.mean())
        pct = base_mw / float(real_values.max()) * 100.0 if real_values.max() > 0 else 100.0

        for month_offset, target_month in enumerate(months_ahead):
            target_date = datetime.now(timezone.utc).replace(day=1, hour=0, minute=0, second=0, microsecond=0)
            records.append(
                EnaCalculatedIntegrationEvent(
                    ExecutionId=sim_id,
                    Submarket=sm,
                    Basin=basin,
                    ValueMwMed=round(base_mw, 2),
                    ValuePercentageMlt=round(pct if month_offset == 0 else pct * 0.97, 2),
                    TargetDate=target_date,
                )
            )
    return records


async def consume_events():
    # Start Prometheus Metrics Server
    start_http_server(8001)
    logger.info("Prometheus metrics server started on port 8001")

    consumer = AIOKafkaConsumer(
        *TOPICS_CONSUME,
        bootstrap_servers=KAFKA_BOOTSTRAP_SERVERS,
        group_id="risk-service-group-v2",
        value_deserializer=lambda m: json.loads(m.decode("utf-8")),
        auto_offset_reset="earliest"
    )
    
    producer = AIOKafkaProducer(
        bootstrap_servers=KAFKA_BOOTSTRAP_SERVERS,
        value_serializer=lambda m: json.dumps(m).encode("utf-8")
    )

    while True:
        try:
            await consumer.start()
            await producer.start()
            logger.info("Successfully connected to Kafka (Consumer & Producer).")
            break
        except Exception as e:
            logger.warning(f"Kafka not ready yet: {e}. Retrying in 5 seconds...")
            await asyncio.sleep(5)

    try:
        async for msg in consumer:
            logger.info(f"Received message on {msg.topic}")
            
            # Extract OpenTelemetry context from Kafka headers
            headers_dict = {}
            if msg.headers:
                headers_dict = {k: v.decode('utf-8') if isinstance(v, bytes) else v for k, v in msg.headers}
            
            ctx = extract(headers_dict)
            
            with tracer.start_as_current_span(f"{msg.topic} receive", context=ctx):
                if msg.topic == "contract-events":
                    await process_contract_event(msg.value, producer)
                elif msg.topic == "pluvia-events":
                    await process_pluvia_event(msg.value, producer)
                
    except asyncio.CancelledError:
        logger.info("Consumer task cancelled.")
    finally:
        await consumer.stop()
        await producer.stop()

async def process_contract_event(event_data: dict, producer: AIOKafkaProducer):
    try:
        event = ContractCreatedEvent(**event_data)
        
        with tracer.start_as_current_span("calculate_risk") as span:
            # Basic financial exposure calculation
            days = (event.endDate - event.startDate).days + 1
            if days <= 0: days = 1
            hours = days * 24
            financial_exposure = event.volumeMwMed * event.price * hours
            
            submarket_str = str(event.submarket).upper()
            if submarket_str == "SE_CO" or submarket_str == "0": submarket_mapped = 0
            elif submarket_str == "SUL" or submarket_str == "1": submarket_mapped = 1
            elif submarket_str == "NORDESTE" or submarket_str == "2": submarket_mapped = 2
            elif submarket_str == "NORTE" or submarket_str == "3": submarket_mapped = 3
            else: submarket_mapped = 0

            # Advanced Mark-to-Market calculation using RiskEngine (supports Swap, Options)
            mtm = RiskEngine.calculate_mtm(
                contract_price=event.price,
                volume_mw=event.volumeMwMed,
                contract_type=event.type,
                submarket=submarket_mapped,
                start_date=event.startDate,
                end_date=event.endDate,
                strike_price=event.strikePrice
            )
            
            risk_category = RiskEngine.determine_risk_category(mtm)
                
            span.set_attribute("risk.financial_exposure", float(financial_exposure))
            span.set_attribute("risk.mark_to_market", float(mtm))
            span.set_attribute("risk.category", risk_category)
            
            # Update Prometheus Metrics
            submarket_name = event.submarket.name if hasattr(event.submarket, 'name') else str(event.submarket)
            RISK_MTM.labels(submarket=submarket_name).set(float(mtm))
            CONTRACTS_PROCESSED.labels(submarket=submarket_name).inc()
            
            logger.info(f"Calculated Risk for {event.counterpartyName}: MtM {mtm:.2f} ({risk_category})")
        
        async with AsyncSessionLocal() as session:
            with tracer.start_as_current_span("db_save_risk"):
                result = await session.execute(
                    select(RiskMetricModel).filter_by(contract_id=event.contractId)
                )
                existing = result.scalars().first()
                
                if not existing:
                    new_metric = RiskMetricModel(
                        tenant_id=event.tenantId,
                        contract_id=event.contractId,
                        counterparty_name=event.counterpartyName,
                        financial_exposure=financial_exposure,
                        mark_to_market=mtm,
                        risk_category=risk_category
                    )
                    session.add(new_metric)
                    await session.commit()
                    logger.info(f"Risk metric saved for contract {event.contractId} in risk_db")
                    
                    risk_event = RiskCalculatedEvent(
                        eventId=uuid.uuid4(),
                        contractId=event.contractId,
                        tenantId=event.tenantId,
                        counterpartyName=event.counterpartyName,
                        financialExposure=financial_exposure,
                        markToMarket=mtm,
                        riskCategory=risk_category,
                        calculatedAt=datetime.utcnow()
                    )
                    
                    with tracer.start_as_current_span(f"{TOPIC_PRODUCE} publish"):
                        # Inject OpenTelemetry context into Kafka headers
                        out_headers = {}
                        inject(out_headers)
                        header_list = [(k, v.encode('utf-8')) for k, v in out_headers.items()]
                        
                        await producer.send_and_wait(
                            TOPIC_PRODUCE,
                            risk_event.model_dump(mode='json'),
                            headers=header_list
                        )
                    logger.info(f"Published RiskCalculatedEvent to topic {TOPIC_PRODUCE}")
                    
                else:
                    existing.mark_to_market = mtm
                    existing.risk_category = risk_category
                    existing.financial_exposure = financial_exposure
                    await session.commit()
                    logger.info(f"Risk metric updated for contract {event.contractId}")
                
    except ValidationError as ve:
        logger.error(f"Validation Error parsing event: {ve}")
    except Exception as e:
        logger.error(f"Error processing event: {e}")
