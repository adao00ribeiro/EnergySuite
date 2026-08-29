"""
Pluvia Price Forecaster - Airflow DAG

Pipeline MLOps que:
1. Lê dados históricos reais (ENA/precipitação/preços) do Data Lake (MinIO).
2. Treina um modelo RandomForest para prever preços de energia.
3. Salva previsões no Data Lake (MinIO) como Parquet e registra artefatos no MLflow.

Se não houver dados reais suficientes, o pipeline encerra com estado claro de
"sem dados" (sem treinar com dados fabricados).

Agendamento: Diário (@daily)
"""
import os
import logging
from datetime import datetime, timedelta

import pandas as pd
import s3fs
import mlflow
import mlflow.sklearn
from sklearn.ensemble import RandomForestRegressor
from sklearn.model_selection import train_test_split
from sklearn.metrics import mean_squared_error, mean_absolute_error, r2_score

from airflow import DAG
from airflow.operators.python import PythonOperator

logger = logging.getLogger(__name__)

# S3 / MinIO Settings
MINIO_ENDPOINT = os.getenv("MINIO_ENDPOINT", "http://minio:9000")
MINIO_ACCESS_KEY = os.getenv("AWS_ACCESS_KEY_ID", "minioadmin")
MINIO_SECRET_KEY = os.getenv("AWS_SECRET_ACCESS_KEY", "minioadmin")
MLFLOW_TRACKING_URI = os.getenv("MLFLOW_TRACKING_URI", "http://mlflow:5000")

SUBMARKETS = {
    0: "SE_CO",
    1: "SUL",
    2: "NE",
    3: "N",
}

DATALAKE_PRICES_PATH = "s3://datalake/prices/latest_prices.parquet"

# Fontes reais de treino (Parquet produzidos por ingestão/risk-service)
HISTORICAL_PATHS = [
    "s3://datalake/gold/energy/historical.parquet",
    "s3://datalake/historical/",
]
METEOROLOGY_PATHS = [
    "s3://datalake/gold/meteorology/precipitation.parquet",
    "s3://datalake/silver/meteorology/precipitation.parquet",
]

MIN_ROWS_FOR_TRAIN = 30


def _get_s3fs():
    return s3fs.S3FileSystem(
        client_kwargs={"endpoint_url": MINIO_ENDPOINT},
        key=MINIO_ACCESS_KEY,
        secret=MINIO_SECRET_KEY,
    )


def _read_historical_frames(fs):
    """Lê DataFrames históricos reais do Data Lake (todos os PARQUET encontrados)."""
    frames = []
    for path in HISTORICAL_PATHS:
        if fs.exists(path):
            if fs.isdir(path):
                for file in fs.ls(path):
                    if file.endswith(".parquet"):
                        try:
                            with fs.open(file, "rb") as f:
                                frames.append(pd.read_parquet(f))
                        except Exception as exc:  # noqa: BLE001
                            logger.warning(f"Falha ao ler {file}: {exc}")
            else:
                try:
                    with fs.open(path, "rb") as f:
                        frames.append(pd.read_parquet(f))
                except Exception as exc:  # noqa: BLE001
                    logger.warning(f"Falha ao ler {path}: {exc}")
    return frames


def extract_real_data(**kwargs):
    """Extrai dados históricos reais de chuva, demanda e preços do Data Lake.

    Nenhuma série aleatória é fabricada. Se nada estiver disponível, publica
    estado NO_DATA via XCom para que o treino não rode com dados falsos.
    """
    ti = kwargs["ti"]
    fs = _get_s3fs()

    frames = _read_historical_frames(fs)

    if not frames:
        logger.warning("[extract] Nenhum dado histórico real no Data Lake.")
        ti.xcom_push(key="state", value="NO_DATA")
        return

    full_df = pd.concat(frames, ignore_index=True)

    required = {"submarket", "price"}
    if not required.issubset(full_df.columns):
        logger.warning("[extract] Dados históricos sem o esquema esperado (submarket/price).")
        ti.xcom_push(key="state", value="NO_DATA")
        return

    full_df = full_df.dropna(subset=["price"])
    full_df["submarket"] = full_df["submarket"].astype(int)

    # Garante colunas de features com valores reais ou derivados deterministicamente
    if "rainfall" not in full_df.columns:
        full_df["rainfall"] = 0.0
    if "demand" not in full_df.columns:
        full_df["demand"] = 0.0

    ts = datetime.now().strftime("%Y%m%d_%H%M%S")
    s3_path = f"s3://datalake/gold/energy/training_{ts}.parquet"
    try:
        with fs.open(s3_path, "wb") as f:
            full_df.to_parquet(f, engine="pyarrow")
        logger.info(f"[extract] Snapshot de treino real salvo em {s3_path}")
    except Exception as exc:  # noqa: BLE001
        logger.warning(f"[extract] Falha ao persistir snapshot: {exc}")

    full_df.to_csv("/tmp/historical_data.csv", index=False)
    logger.info(f"[extract] Extração real concluída. {len(full_df)} linhas.")

    ti.xcom_push(key="state", value="TRAIN")
    ti.xcom_push(key="row_count", value=len(full_df))


def train_model(**kwargs):
    """Treina o modelo RandomForest sobre dados reais e registra no MLflow.

    Se o estado for NO_DATA, registra estado claro sem treinar com mock.
    """
    ti = kwargs["ti"]
    state = ti.xcom_pull(task_ids="extract_real_data", key="state")

    mlflow.set_tracking_uri(MLFLOW_TRACKING_URI)
    mlflow.set_experiment("pluvia_price_forecaster")

    if state != "TRAIN":
        with mlflow.start_run(run_name=f"rf_daily_{datetime.now().strftime('%Y%m%d')}"):
            mlflow.log_param("train_state", "NO_DATA")
            mlflow.log_metric("train_samples", 0)
        logger.warning("[train] Nenhum dado real. Treino pulado (estado NO_DATA).")
        ti.xcom_push(key="mse", value=None)
        ti.xcom_push(key="r2", value=None)
        return

    df = pd.read_csv("/tmp/historical_data.csv")

    X = df[["submarket", "rainfall", "demand"]].apply(pd.to_numeric, errors="coerce").fillna(0)
    y = pd.to_numeric(df["price"], errors="coerce")

    X, y = X[y.notna()], y[y.notna()]

    if len(X) < MIN_ROWS_FOR_TRAIN:
        with mlflow.start_run(run_name=f"rf_daily_{datetime.now().strftime('%Y%m%d')}"):
            mlflow.log_param("train_state", "NO_DATA")
            mlflow.log_metric("train_samples", len(X))
        logger.warning(f"[train] Poucas linhas reais ({len(X)}). Sem treino com mock.")
        ti.xcom_push(key="mse", value=None)
        ti.xcom_push(key="r2", value=None)
        return

    X_train, X_test, y_train, y_test = train_test_split(
        X, y, test_size=0.2, random_state=42
    )

    with mlflow.start_run(run_name=f"rf_daily_{datetime.now().strftime('%Y%m%d')}"):
        model = RandomForestRegressor(n_estimators=100, max_depth=8, random_state=42)
        model.fit(X_train, y_train)

        predictions = model.predict(X_test)
        mse = mean_squared_error(y_test, predictions)
        mae = mean_absolute_error(y_test, predictions)
        r2 = r2_score(y_test, predictions)

        logger.info(f"Model trained (dados reais). MSE={mse:.4f} MAE={mae:.4f} R2={r2:.4f}")

        mlflow.log_param("train_state", "TRAINED")
        mlflow.log_param("n_estimators", 100)
        mlflow.log_param("max_depth", 8)
        mlflow.log_param("model_type", "RandomForestRegressor")
        mlflow.log_param("train_samples", len(X_train))
        mlflow.log_param("test_samples", len(X_test))

        mlflow.log_metric("mse", mse)
        mlflow.log_metric("mae", mae)
        mlflow.log_metric("r2", r2)

        for i, col in enumerate(X.columns):
            mlflow.log_metric(f"feature_importance_{col}", model.feature_importances_[i])

        mlflow.sklearn.log_model(model, "random_forest_model")

        import joblib

        joblib.dump(model, "/tmp/latest_model.pkl")
        logger.info("Model saved to /tmp/latest_model.pkl")

        ti.xcom_push(key="mse", value=mse)
        ti.xcom_push(key="r2", value=r2)


def predict_and_load_datalake(**kwargs):
    """Gera previsões com base em dados reais de entrada (não aleatórios)."""
    import joblib

    ti = kwargs["ti"]
    state = ti.xcom_pull(task_ids="extract_real_data", key="state")

    if state != "TRAIN":
        logger.warning("[predict] Sem dados reais de treino. Publicação de previsão pulada.")
        return

    if not os.path.exists("/tmp/latest_model.pkl"):
        logger.warning("[predict] Modelo não treinado. Pulando previsão.")
        return

    model = joblib.load("/tmp/latest_model.pkl")

    # Entradas reais: última precipitação persistida por submercado no Data Lake.
    rainfall_map = _latest_meteorology_by_submarket()

    today_predictions = []
    for submarket_id, sub_name in SUBMARKETS.items():
        current_rain = rainfall_map.get(submarket_id, 0.0)
        current_demand = 0.0  # demanda real seria lida de fonte de dados dedicada

        X_today = pd.DataFrame(
            {
                "submarket": [submarket_id],
                "rainfall": [current_rain],
                "demand": [current_demand],
            }
        )

        predicted_price = float(model.predict(X_today)[0])

        today_predictions.append(
            {
                "submarket_id": submarket_id,
                "submarket_name": sub_name,
                "base_price": round(predicted_price, 2),
                "volatility": 0.0,
                "rainfall_index": round(current_rain, 2),
                "demand_mw": round(current_demand, 2),
                "date": datetime.now().date().isoformat(),
                "model_version": datetime.now().strftime("%Y%m%d"),
            }
        )

    results_df = pd.DataFrame(today_predictions)

    fs = _get_s3fs()
    try:
        fs.mkdir("datalake/prices")
    except Exception:  # noqa: BLE001
        pass

    with fs.open(DATALAKE_PRICES_PATH, "wb") as f:
        results_df.to_parquet(f, engine="pyarrow")

    logger.info(f"Prices saved to Data Lake at {DATALAKE_PRICES_PATH}")
    logger.info(results_df.to_string())

    ti.xcom_push(key="predictions", value=today_predictions)


def _latest_meteorology_by_submarket():
    """Retorna a precipitação real mais recente agregada por submercado (vetorizado).

    Deriva o submercado a partir da coluna `submarket` quando presente, senão por
    faixa de latitude (proxy geográfico determinístico).
    """
    fs = _get_s3fs()
    for path in METEOROLOGY_PATHS:
        if not fs.exists(path):
            continue
        try:
            with fs.open(path, "rb") as f:
                df = pd.read_parquet(f)
        except Exception as exc:  # noqa: BLE001
            logger.warning(f"Falha ao ler meteorologia {path}: {exc}")
            continue

        if df.empty:
            continue

        if "submarket" not in df.columns and "lat" in df.columns:
            df["submarket"] = pd.cut(df["lat"], bins=4, labels=range(4)).astype(int)

        if "submarket" not in df.columns:
            continue

        df = df.groupby("submarket", as_index=False)["value_mm"].mean()
        return {int(row["submarket"]): float(row["value_mm"]) for _, row in df.iterrows()}

    return {}


default_args = {
    "owner": "airflow",
    "depends_on_past": False,
    "start_date": datetime(2026, 1, 1),
    "retries": 2,
    "retry_delay": timedelta(minutes=5),
    "email_on_failure": False,
}

with DAG(
    "train_price_model",
    default_args=default_args,
    description="Pluvia: treina modelo de previsão de preços sobre dados reais e publica no Data Lake",
    schedule_interval="@daily",
    catchup=False,
    tags=["mlops", "pluvia", "pricing"],
) as dag:

    t1 = PythonOperator(
        task_id="extract_real_data",
        python_callable=extract_real_data,
    )

    t2 = PythonOperator(
        task_id="train_model",
        python_callable=train_model,
    )

    t3 = PythonOperator(
        task_id="predict_and_load_datalake",
        python_callable=predict_and_load_datalake,
    )

    t1 >> t2 >> t3
