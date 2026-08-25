import asyncio
from contextlib import asynccontextmanager
from fastapi import FastAPI
from .kafka_consumer import consume_events

@asynccontextmanager
async def lifespan(app: FastAPI):
    # Start Kafka consumer as a background task
    task = asyncio.create_task(consume_events())
    yield
    # Cancel the task when shutting down
    task.cancel()
    try:
        await task
    except asyncio.CancelledError:
        pass

app = FastAPI(title="Risk & Prospec Service", lifespan=lifespan)

@app.get("/health")
async def health_check():
    return {"status": "ok"}
