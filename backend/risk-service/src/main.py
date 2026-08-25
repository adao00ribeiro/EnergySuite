import asyncio
from contextlib import asynccontextmanager
from fastapi import FastAPI, Depends, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy.future import select
import uuid

from .kafka_consumer import consume_events
from .database import engine, Base, get_db
from .models import RiskMetricModel, RiskMetricResponse

@asynccontextmanager
async def lifespan(app: FastAPI):
    # Initialize Database Tables
    async with engine.begin() as conn:
        await conn.run_sync(Base.metadata.create_all)
        
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

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"], # For development
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.get("/health")
async def health_check():
    return {"status": "ok"}

@app.get("/api/v1/metrics/contracts/{contract_id}", response_model=RiskMetricResponse)
async def get_risk_metric(contract_id: uuid.UUID, db: AsyncSession = Depends(get_db)):
    result = await db.execute(select(RiskMetricModel).filter_by(contract_id=contract_id))
    metric = result.scalars().first()
    
    if not metric:
        raise HTTPException(status_code=404, detail="Risk metric not found for this contract")
        
    return metric
