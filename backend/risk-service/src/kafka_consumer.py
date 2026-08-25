import json
import os
import asyncio
import logging
import uuid
from datetime import datetime
from aiokafka import AIOKafkaConsumer, AIOKafkaProducer
from pydantic import ValidationError
from sqlalchemy.future import select

from opentelemetry import trace
from opentelemetry.propagate import extract, inject

from .database import AsyncSessionLocal
from .models import ContractCreatedEvent, RiskMetricModel, RiskCalculatedEvent
from .risk_engine import RiskEngine

from prometheus_client import Gauge, Counter, start_http_server

# Prometheus Metrics
RISK_MTM = Gauge('risk_mtm_value', 'Mark-to-Market value', ['submarket'])
CONTRACTS_PROCESSED = Counter('risk_contracts_processed_total', 'Total contracts processed', ['submarket'])

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

tracer = trace.get_tracer(__name__)

KAFKA_BOOTSTRAP_SERVERS = os.getenv("KAFKA_BOOTSTRAP_SERVERS", "kafka:9092")
TOPIC_CONSUME = "contract-events"
TOPIC_PRODUCE = "risk-events"

async def consume_events():
    # Start Prometheus Metrics Server
    start_http_server(8001)
    logger.info("Prometheus metrics server started on port 8001")

    consumer = AIOKafkaConsumer(
        TOPIC_CONSUME,
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
                await process_contract_event(msg.value, producer)
                
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
