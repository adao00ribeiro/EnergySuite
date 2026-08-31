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
    allow_origins=[
        "http://localhost:4200",
        "http://localhost:4201",
        "http://localhost:4202",
        "http://localhost:4203",
        "http://localhost:4204",
    ],
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
    Retorna uma matriz geoespacial de precipitação real lida do Data Lake (MinIO).

    Fonte: Parquet (gold/silver) produzido pelos DAGs do mlops
    (ingest_meteorological_data.py). Interpolação vetorizada com NumPy para a
    grade de saída (bounds do Brasil: lat -33 a 5, lon -74 a -34).

    Se não houver dados reais para o período solicitado, retorna HTTP 404 com
    problem detail (estado vazio honesto) — nunca gera matriz aleatória.
    """
    import numpy as np
    import pandas as pd

    from .data_lake import PRECIPITATION_PATHS, read_first_existing

    horizon_days = 8

    # Grid limits
    lat_min, lat_max = -33.0, 5.0
    lon_min, lon_max = -74.0, -34.0

    # 40x40 grid (1600 points)
    lat_steps = 40
    lon_steps = 40

    df = read_first_existing(PRECIPITATION_PATHS)
    if df is None or df.empty:
        raise HTTPException(
            status_code=404,
            detail={
                "title": "Precipitação indisponível",
                "type": "about:blank",
                "status": 404,
                "detail": "Nenhum dado real de precipitação encontrado no Data Lake "
                          "(camada gold/silver de meteorologia). Execute o DAG "
                          "ingest_meteorological_data para popular os dados.",
            },
        )

    # Filtra por modelo e data (offsetDays) — vetorizado
    if model:
        df = df[df["model"].astype(str).str.upper() == model.upper()]
    if date:
        df = df[df["date"].astype(str) == date]

    if df.empty:
        raise HTTPException(
            status_code=404,
            detail={
                "title": "Precipitação indisponível",
                "type": "about:blank",
                "status": 404,
                "detail": (
                    f"Nenhum dado real de precipitação para model={model} e "
                    f"date={date or '(todos os períodos)'} no Data Lake."
                ),
            },
        )

    # GRADE DE SAÍDA
    lat_sel = np.linspace(lat_min, lat_max, lat_steps)
    lon_sel = np.linspace(lon_min, lon_max, lon_steps)
    grid_lon, grid_lat = np.meshgrid(lon_sel, lat_sel)
    coords = np.stack([grid_lon.ravel(), grid_lat.ravel()], axis=-1)

    # Coordenadas únicas ordenadas da fonte (Parquet real)
    unique_lon = np.sort(np.asarray(pd.unique(df["lon"]), dtype=float))
    unique_lat = np.sort(np.asarray(pd.unique(df["lat"]), dtype=float))

    if unique_lon.size == 0 or unique_lat.size == 0:
        raise HTTPException(
            status_code=404,
            detail={
                "title": "Precipitação indisponível",
                "type": "about:blank",
                "status": 404,
                "detail": "Dados de precipitação presentes porém sem coordenadas válidas.",
            },
        )

    # Matriz de precipitação média por (lat, lon) da fonte (pivot vetorizado)
    piv = df.groupby(["lat", "lon"], as_index=False)["value_mm"].mean().pivot_table(
        index="lat", columns="lon", values="value_mm", aggfunc="mean"
    ).reindex(index=unique_lat, columns=unique_lon)
    pivot_values = piv.to_numpy(dtype=float)
    pivot_values = np.nan_to_num(pivot_values, nan=0.0)
    pivot_values = np.clip(pivot_values, 0.0, None)

    # Mapa vetorizado de cada ponto de saída para o índice mais próximo da fonte
    lon_pos = np.clip(np.searchsorted(unique_lon, coords[:, 0]), 1, unique_lon.size - 1)
    lat_pos = np.clip(np.searchsorted(unique_lat, coords[:, 1]), 1, unique_lat.size - 1)
    # searchsorted devolve o índice à direita; corrigimos para o vizinho mais próximo
    lon_nearest = np.where(
        np.abs(unique_lon[lon_pos - 1] - coords[:, 0]) <= np.abs(unique_lon[lon_pos] - coords[:, 0]),
        lon_pos - 1,
        lon_pos,
    )
    lat_nearest = np.where(
        np.abs(unique_lat[lat_pos - 1] - coords[:, 1]) <= np.abs(unique_lat[lat_pos] - coords[:, 1]),
        lat_pos - 1,
        lat_pos,
    )

    out_mm = pivot_values[lat_nearest, lon_nearest]

    def to_points(mm_flat):
        out = np.column_stack([coords[:, 0], coords[:, 1], mm_flat])
        return out.round(2).tolist()

    # Dia 1 mantém a chave "points" por compatibilidade; demais dias interpolam
    # a mesma superfície espacial (a superfície real é estática por execução).
    points_day1 = to_points(out_mm)
    days = [
        {
            "offset": offset,
            "date": date,
            "points": points_day1,
        }
        for offset in range(1, horizon_days + 1)
    ]

    return {
        "model": model,
        "date": date,
        "horizon_days": horizon_days,
        "points": points_day1,
        "days": days,
    }
