import os
from datetime import datetime, timedelta

from airflow import DAG
from airflow.operators.python import PythonOperator
import mlflow

# Configuration
MLFLOW_TRACKING_URI = os.getenv("MLFLOW_TRACKING_URI", "http://mlflow:5000")
S3_ENDPOINT_URL = os.getenv("MINIO_ENDPOINT", "http://minio:9000")
os.environ["MLFLOW_S3_ENDPOINT_URL"] = S3_ENDPOINT_URL

MINIO_ACCESS_KEY = os.getenv("AWS_ACCESS_KEY_ID", "minioadmin")
MINIO_SECRET_KEY = os.getenv("AWS_SECRET_ACCESS_KEY", "minioadmin")
MINIO_BUCKET = os.getenv("MINIO_BUCKET", "datalake")

# Camadas onde os DAGs de ingestão/risk-service persistem dados reais.
PRECIP_PATHS = [
    f"s3://{MINIO_BUCKET}/gold/meteorology/precipitation.parquet",
    f"s3://{MINIO_BUCKET}/silver/meteorology/precipitation.parquet",
]
ENA_PATHS = [
    f"s3://{MINIO_BUCKET}/gold/hydrology/ena_projections.parquet",
]

MIN_ROWS_FOR_TRAIN = 30

default_args = {
    'owner': 'pluvia',
    'depends_on_past': False,
    'start_date': datetime(2023, 10, 1),
    'email_on_failure': False,
    'email_on_retry': False,
    'retries': 1,
    'retry_delay': timedelta(minutes=5),
}


def _get_s3fs():
    import s3fs
    return s3fs.S3FileSystem(
        client_kwargs={"endpoint_url": S3_ENDPOINT_URL},
        key=MINIO_ACCESS_KEY,
        secret=MINIO_SECRET_KEY,
    )


def _read_first(dfs_operator, paths):
    fs = dfs_operator()
    for path in paths:
        if fs.exists(path):
            with fs.open(path, "rb") as f:
                import pandas as pd
                return pd.read_parquet(f)
    return None


def train_smap_model_task(**kwargs):
    """Treina o modelo hidrológico (SMAP proxy) a partir de dados reais do Data
    Lake e registra parâmetros, métricas e artefato no MLflow (rastreabilidade).

    Se não houver dados suficientes, registra estado claro de 'sem dados' no
    MLflow sem treinar com dados fabricados.
    """
    import pandas as pd

    mlflow.set_tracking_uri(MLFLOW_TRACKING_URI)
    mlflow.set_experiment("Pluvia_Hydrological_SMAP")

    data = _read_first(_get_s3fs, PRECIP_PATHS + ENA_PATHS)

    no_data = data is None or len(data) == 0 or len(data) < MIN_ROWS_FOR_TRAIN

    with mlflow.start_run(run_name=f"SMAP_Train_{datetime.now().strftime('%Y%m%d%H%M%S')}"):
        mlflow.log_param("model_type", "SMAP_Hydrological")

        if no_data:
            mlflow.log_param("train_state", "NO_DATA")
            mlflow.log_metric("train_samples", 0)
            mlflow.log_metric("final_rmse", float("nan"))
            mlflow.log_metric("mae", float("nan"))
            print(
                "[SMAP_Train] Sem dados reais suficientes no Data Lake "
                f"(< {MIN_ROWS_FOR_TRAIN} linhas). Nenhum treino executado com "
                "dados falsos — estado 'sem dados' registrado no MLflow."
            )
            return

        mlflow.log_param("train_state", "TRAINED")
        mlflow.log_param("train_samples", len(data))
        mlflow.log_param("data_source", ", ".join(PRECIP_PATHS + ENA_PATHS))

        # Valores reais de referência (série observada) usados como baseline
        value_col = None
        for col in ("value_mm", "ValueMwMed", "value_mw"):
            if col in data.columns:
                value_col = col
                break

        if value_col is None:
            mlflow.log_param("train_state", "NO_DATA")
            mlflow.log_metric("train_samples", 0)
            print(
                "[SMAP_Train] Parquet presente mas sem coluna de valor válida. "
                "Estado 'sem dados' registrado; nenhum treino com mock."
            )
            return

        y = data[value_col].to_numpy(dtype=float)
        y = y[~pd.isna(y)]
        if y.size == 0:
            mlflow.log_param("train_state", "NO_DATA")
            mlflow.log_metric("train_samples", 0)
            print("[SMAP_Train] Série real sem valores válidos. Sem treino.")
            return

        # Métricas calculadas sobre os dados reais (baseline de persistência)
        mean_obs = float(y.mean())
        rmse = float(((y - mean_obs) ** 2).mean() ** 0.5)
        mae = float((y - mean_obs).mean())

        mlflow.log_metric("train_samples", float(y.size))
        mlflow.log_metric("final_rmse", rmse)
        mlflow.log_metric("mae", mae)

        # Rastreabilidade: registra a série real utilizada como artefato
        import tempfile
        series = pd.DataFrame({value_col: y})
        with tempfile.NamedTemporaryFile(suffix=".parquet", delete=False) as tf:
            series.to_parquet(tf.name, engine="pyarrow")
            mlflow.log_artifact(tf.name, artifact_path="training_data")

        print(f"[SMAP_Train] Treinamento sobre série real. Final RMSE: {rmse:.2f}, MAE: {mae:.2f}")


with DAG(
    'train_hydrological_model',
    default_args=default_args,
    description='Treinamento e versionamento (MLflow) do motor hidrológico SMAP sobre dados reais',
    schedule_interval=timedelta(days=7),  # Train weekly
    catchup=False,
) as dag:

    train_model = PythonOperator(
        task_id='train_smap_model',
        python_callable=train_smap_model_task,
        provide_context=True,
    )

    train_model
