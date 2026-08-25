import json
import os
import asyncio
import logging
import uuid
from datetime import datetime
from aiokafka import AIOKafkaConsumer, AIOKafkaProducer
from pydantic import ValidationError
from sqlalchemy.future import select

from .database import AsyncSessionLocal
from .models import ContractCreatedEvent, RiskMetricModel, RiskCalculatedEvent

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

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

    # Retry loop to wait for Kafka to be ready
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
            await process_contract_event(msg.value, producer)
    except asyncio.CancelledError:
        logger.info("Consumer task cancelled.")
    finally:
        await consumer.stop()
        await producer.stop()

async def process_contract_event(event_data: dict, producer: AIOKafkaProducer):
    try:
        event = ContractCreatedEvent(**event_data)
        
        financial_exposure = event.volumeMwMed * event.price * 720
        
        if financial_exposure > 5000000:
            risk_category = "HIGH"
        elif financial_exposure > 1000000:
            risk_category = "MEDIUM"
        else:
            risk_category = "LOW"
            
        logger.info(f"Calculated Risk for {event.counterpartyName}: {financial_exposure} ({risk_category})")
        
        async with AsyncSessionLocal() as session:
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
                
                # Publish Event to Kafka for BI
                risk_event = RiskCalculatedEvent(
                    eventId=uuid.uuid4(),
                    contractId=event.contractId,
                    counterpartyName=event.counterpartyName,
                    financialExposure=financial_exposure,
                    riskCategory=risk_category,
                    calculatedAt=datetime.utcnow()
                )
                
                await producer.send_and_wait(
                    TOPIC_PRODUCE,
                    # We serialize dict with json compatible format (e.g. stringify uuids and datetimes)
                    # Pydantic's model_dump() doesn't auto-convert datetimes to str unless we use jsonable_encoder or custom serialize.
                    # Pydantic V2 allows model_dump_json() which returns a string, then we can parse it back to dict for the producer's serializer, 
                    # OR we can just pass the string to the producer if we change the serializer to expect str.
                    # Let's just use model_dump_json() and pass it directly.
                    # Wait, the producer expects dict and uses json.dumps. So we can use model_dump(mode='json').
                    risk_event.model_dump(mode='json')
                )
                logger.info(f"Published RiskCalculatedEvent to topic {TOPIC_PRODUCE}")
                
            else:
                logger.info(f"Risk metric already exists for contract {event.contractId}")
                
    except ValidationError as ve:
        logger.error(f"Validation Error parsing event: {ve}")
    except Exception as e:
        logger.error(f"Error processing event: {e}")
