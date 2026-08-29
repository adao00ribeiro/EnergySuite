"""Data Lake (MinIO) access helpers.

Centraliza a leitura/escrita de dados científicos reais persistidos em Parquet
no MinIO (bucket `datalake`). Nenhum dado aqui é fabricado: se a fonte não
existir, os consumidores devem registrar estado vazio/erro honesto.
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
    logger.info(f"Lendo dados reais do Data Lake: {path}")
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
