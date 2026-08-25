import json
import os
import asyncio
import logging
from aiokafka import AIOKafkaConsumer
from pydantic import ValidationError
from sqlalchemy.future import select

from .database import AsyncSessionLocal
from .models import ContractCreatedEvent, RiskMetricModel

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

KAFKA_BOOTSTRAP_SERVERS = os.getenv("KAFKA_BOOTSTRAP_SERVERS", "kafka:9092")
TOPIC = "contract-events"

async def consume_events():
    consumer = AIOKafkaConsumer(
        TOPIC,
        bootstrap_servers=KAFKA_BOOTSTRAP_SERVERS,
        group_id="risk-service-group",
        value_deserializer=lambda m: json.loads(m.decode("utf-8")),
        auto_offset_reset="earliest"
    )

    # Retry loop to wait for Kafka to be ready
    while True:
        try:
            await consumer.start()
            logger.info("Successfully connected to Kafka.")
            break
        except Exception as e:
            logger.warning(f"Kafka not ready yet: {e}. Retrying in 5 seconds...")
            await asyncio.sleep(5)

    try:
        async for msg in consumer:
            logger.info(f"Received message on {msg.topic}")
            await process_contract_event(msg.value)
    except asyncio.CancelledError:
        logger.info("Consumer task cancelled.")
    finally:
        await consumer.stop()

async def process_contract_event(event_data: dict):
    # Convert .NET PascalCase to camelCase if needed, but our Pydantic schema expects exact matches.
    # We must ensure the keys match the ContractCreatedEvent schema.
    try:
        # Pydantic is case sensitive by default, but we defined it as camelCase or PascalCase.
        # .NET serialized as PascalCase if we didn't specify camelCase for the MassTransit event,
        # but wait, we added CamelCase global JSON option. Let's see if MassTransit respects it.
        # To be safe, we can try to construct it. If it fails, log it.
        
        # In .NET we used CamelCase policy globally, but MassTransit defaults to camelCase too.
        # Let's map it manually to be safe, or just pass it to Pydantic if it matches.
        event = ContractCreatedEvent(**event_data)
        
        # Calculate Risk (Simulated)
        # Financial Exposure = Volume * Price * 720 hours (1 month roughly)
        financial_exposure = event.volumeMwMed * event.price * 720
        
        if financial_exposure > 5000000:
            risk_category = "HIGH"
        elif financial_exposure > 1000000:
            risk_category = "MEDIUM"
        else:
            risk_category = "LOW"
            
        logger.info(f"Calculated Risk for {event.counterpartyName}: {financial_exposure} ({risk_category})")
        
        # Save to Database
        async with AsyncSessionLocal() as session:
            # Check if exists
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
                logger.info(f"Risk metric saved for contract {event.contractId}")
            else:
                logger.info(f"Risk metric already exists for contract {event.contractId}")
                
    except ValidationError as ve:
        logger.error(f"Validation Error parsing event: {ve}")
    except Exception as e:
        logger.error(f"Error processing event: {e}")
