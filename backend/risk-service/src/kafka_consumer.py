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

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

tracer = trace.get_tracer(__name__)

KAFKA_BOOTSTRAP_SERVERS = os.getenv("KAFKA_BOOTSTRAP_SERVERS", "kafka:9092")
TOPIC_CONSUME = "contract-events"
TOPIC_PRODUCE = "risk-events"

async def consume_events():
    consumer = AIOKafkaConsumer(
        TOPIC_CONSUME,
        bootstrap_servers=KAFKA_BOOTSTRAP_SERVERS,
        group_id="risk-service-group",
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
            financial_exposure = event.volumeMwMed * event.price * 720
            
            if financial_exposure > 5000000:
                risk_category = "HIGH"
            elif financial_exposure > 1000000:
                risk_category = "MEDIUM"
            else:
                risk_category = "LOW"
                
            span.set_attribute("risk.financial_exposure", float(financial_exposure))
            span.set_attribute("risk.category", risk_category)
            
            logger.info(f"Calculated Risk for {event.counterpartyName}: {financial_exposure} ({risk_category})")
        
        async with AsyncSessionLocal() as session:
            with tracer.start_as_current_span("db_save_risk"):
                result = await session.execute(
                    select(RiskMetricModel).filter_by(contract_id=event.contractId)
                )
                existing = result.scalars().first()
                
                if not existing:
                    new_metric = RiskMetricModel(
                        contract_id=event.contractId,
                        counterparty_name=event.counterpartyName,
                        financial_exposure=financial_exposure,
                        risk_category=risk_category
                    )
                    session.add(new_metric)
                    await session.commit()
                    logger.info(f"Risk metric saved for contract {event.contractId} in risk_db")
                    
                    risk_event = RiskCalculatedEvent(
                        eventId=uuid.uuid4(),
                        contractId=event.contractId,
                        counterpartyName=event.counterpartyName,
                        financialExposure=financial_exposure,
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
                    logger.info(f"Risk metric already exists for contract {event.contractId}")
                
    except ValidationError as ve:
        logger.error(f"Validation Error parsing event: {ve}")
    except Exception as e:
        logger.error(f"Error processing event: {e}")
