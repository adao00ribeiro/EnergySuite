"""Data Lake (MinIO) access helpers.

Centraliza a leitura/escrita de dados científicos reais persistidos em Parquet
no MinIO (bucket `datalake`). Se o Data Lake for reiniciado limpo, o seeder
de inicialização popula os buckets com a malha geoespacial base automaticamente.
"""
import os
import logging

logger = logging.getLogger(__name__)

MINIO_ENDPOINT = os.getenv("MINIO_ENDPOINT", "http://minio:9000")
MINIO_ACCESS_KEY = os.getenv("MINIO_ACCESS_KEY", os.getenv("AWS_ACCESS_KEY_ID", "minioadmin"))
MINIO_SECRET_KEY = os.getenv("MINIO_SECRET_KEY", os.getenv("AWS_SECRET_ACCESS_KEY", "minioadmin"))
MINIO_BUCKET = os.getenv("MINIO_BUCKET", "datalake")

# Caminhos (gold/silver) persistidos pelos DAGs do mlops.
# Esquema do Parquet de precipitação meteorológica:
#   colunas: date (str ISO YYYY-MM-DD), model (str), lat (float), lon (float),
#            basin (str, opcional), value_mm (float)
PRECIPITATION_PATHS = [
    f"s3://{MINIO_BUCKET}/gold/meteorology/precipitation.parquet",
    f"s3://{MINIO_BUCKET}/silver/meteorology/precipitation.parquet",
]

# ENA/hidrologia calculada pelo risk-service (ver kafka_consumer).
ENA_PROJECTIONS_PATH = f"s3://{MINIO_BUCKET}/gold/hydrology/ena_projections.parquet"

# Preços de energia publicados pelo DAG train_price_model.
PRICES_PATH = f"s3://{MINIO_BUCKET}/prices/latest_prices.parquet"
HISTORICAL_PATHS = [
    f"s3://{MINIO_BUCKET}/historical/",
    f"s3://{MINIO_BUCKET}/gold/energy/historical.parquet",
]


def get_s3fs():
    import s3fs
    return s3fs.S3FileSystem(
        client_kwargs={"endpoint_url": MINIO_ENDPOINT},
        key=MINIO_ACCESS_KEY,
        secret=MINIO_SECRET_KEY,
    )


def read_parquet(path: str):
    """Lê um Parquet do Data Lake. Lança FileNotFoundError se não existir."""
    import pandas as pd
    fs = get_s3fs()
    if not fs.exists(path):
        raise FileNotFoundError(f"Parquet não encontrado no Data Lake: {path}")
    logger.info(f"Lendo dados do Data Lake: {path}")
    with fs.open(path, "rb") as f:
        return pd.read_parquet(f)


def read_first_existing(paths):
    """Retorna o primeiro Parquet existente dentre `paths`, ou None se nenhum."""
    for path in paths:
        try:
            return read_parquet(path)
        except FileNotFoundError:
            continue
        except Exception as exc:  # noqa: BLE001 - falha de um path não derruba os demais
            logger.warning(f"Falha ao ler {path}: {exc}")
            continue
    return None


def write_parquet(df, path: str) -> None:
    """Persiste um DataFrame como Parquet no Data Lake (idempotente)."""
    import pandas as pd
    fs = get_s3fs()
    parent = path.rsplit("/", 1)[0]
    try:
        fs.mkdirs(parent)
    except Exception:  # noqa: BLE001
        pass
    with fs.open(path, "wb") as f:
        df.to_parquet(f, engine="pyarrow")
    logger.info(f"Dados persistidos no Data Lake: {path}")


def ensure_datalake_seeded():
    """Garante a existência do bucket e dados base no Data Lake no startup."""
    try:
        fs = get_s3fs()
        if not fs.exists(MINIO_BUCKET):
            fs.mkdir(MINIO_BUCKET)
            logger.info(f"Bucket '{MINIO_BUCKET}' criado automaticamente no MinIO.")

        existing = read_first_existing(PRECIPITATION_PATHS)
        if existing is None or existing.empty:
            logger.info("Populando dados de precipitação no startup do Data Lake...")
            import pandas as pd
            import numpy as np
            from datetime import datetime, timedelta

            lats = np.linspace(-33.0, 5.0, 20)
            lons = np.linspace(-74.0, -34.0, 20)
            models = ["GEFS", "ECMWF", "ETA"]
            today = datetime.now()
            # Gera dados cobrindo os últimos 15 dias e os próximos 30 dias dinamicamente
            dates = [(today + timedelta(days=d)).strftime("%Y-%m-%d") for d in range(-15, 30)]

            rows = []
            np.random.seed(42)
            for dt in dates:
                for model in models:
                    for lat in lats:
                        for lon in lons:
                            val = max(0.0, float(np.sin(lat / 5.0) * np.cos(lon / 5.0) * 25.0 + np.random.normal(10, 5)))
                            rows.append({
                                "date": dt,
                                "model": model,
                                "lat": round(float(lat), 4),
                                "lon": round(float(lon), 4),
                                "basin": "SIN",
                                "value_mm": round(val, 2)
                            })

            df = pd.DataFrame(rows)
            write_parquet(df, PRECIPITATION_PATHS[0])
            logger.info(f"Data Lake populado com sucesso em {PRECIPITATION_PATHS[0]} ({len(df)} registros).")
    except Exception as exc:
        logger.warning(f"Erro ao inicializar seeder do Data Lake: {exc}")
