import asyncio
from contextlib import asynccontextmanager
from fastapi import FastAPI, Depends, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy.future import select
import uuid

import os
from .kafka_consumer import consume_events
from .database import engine, Base, get_db
from .models import RiskMetricModel, RiskMetricResponse
from .auth import verify_jwt
from sqlalchemy import func

from opentelemetry import trace
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor
from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter
from opentelemetry.sdk.resources import Resource
from opentelemetry.instrumentation.fastapi import FastAPIInstrumentor
from opentelemetry.instrumentation.sqlalchemy import SQLAlchemyInstrumentor

resource = Resource.create({"service.name": "risk-service"})
trace.set_tracer_provider(TracerProvider(resource=resource))
otlp_exporter = OTLPSpanExporter(endpoint=os.getenv("OTEL_EXPORTER_OTLP_ENDPOINT", "http://tempo:4317"), insecure=True)
trace.get_tracer_provider().add_span_processor(BatchSpanProcessor(otlp_exporter))

SQLAlchemyInstrumentor().instrument(engine=engine.sync_engine)

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
FastAPIInstrumentor.instrument_app(app)

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

@app.get("/api/v1/metrics/portfolio")
async def get_portfolio_risk(db: AsyncSession = Depends(get_db), token_payload: dict = Depends(verify_jwt)):
    # Simula extração do tenant_id do token, fallback para default dev
    tenant_claim = token_payload.get("tenant_id", token_payload.get("azp", "00000000-0000-0000-0000-000000000001"))
    try:
        tenant_id = uuid.UUID(tenant_claim)
    except:
        tenant_id = uuid.UUID("00000000-0000-0000-0000-000000000001")
        
    query = select(
        RiskMetricModel.counterparty_name,
        func.sum(RiskMetricModel.financial_exposure).label('total_exposure'),
        func.sum(RiskMetricModel.mark_to_market).label('total_mtm')
    ).filter(
        RiskMetricModel.tenant_id == tenant_id
    ).group_by(
        RiskMetricModel.counterparty_name
    )
    
    result = await db.execute(query)
    rows = result.all()
    
    portfolio = []
    for row in rows:
        portfolio.append({
            "counterparty_name": row.counterparty_name,
            "financial_exposure": float(row.total_exposure),
            "mark_to_market": float(row.total_mtm)
        })
        
    return portfolio

@app.get("/api/v1/pluvia/precipitation-map")
async def get_precipitation_map(model: str = "GEFS", date: str = ""):
    """
    Retorna uma matriz geoespacial de precipitação (mockada para a Sprint 2).
    Simula o processamento dos arquivos GRIB2 do Data Lakehouse.
    Bounds do Brasil: Lat -33 a 5, Lon -74 a -34
    """
    import random
    
    data = []
    # Grid limits
    lat_min, lat_max = -33.0, 5.0
    lon_min, lon_max = -74.0, -34.0
    
    # 40x40 grid (1600 points)
    lat_steps = 40
    lon_steps = 40
    
    lat_step_size = (lat_max - lat_min) / lat_steps
    lon_step_size = (lon_max - lon_min) / lon_steps
    
    # Random seed based on model to show differences when filtering
    seed_val = hash(model + date)
    random.seed(seed_val)
    
    for i in range(lat_steps):
        for j in range(lon_steps):
            lat = lat_min + (i * lat_step_size)
            lon = lon_min + (j * lon_step_size)
            
            # Gerar valores de chuva mais realistas (0 a 100mm)
            # Maior chance de zero, e bolsões de chuva
            if random.random() > 0.6:
                precip = random.uniform(5.0, 100.0)
            else:
                precip = 0.0
                
            # ECharts scatter format: [lon, lat, value]
            data.append([round(lon, 2), round(lat, 2), round(precip, 2)])
            
    return {
        "model": model,
        "date": date,
        "points": data
    }
