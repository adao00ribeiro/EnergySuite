"""
Pluvia Price Forecaster - Airflow DAG

Pipeline MLOps que:
1. Extrai/simula dados históricos de chuva, demanda e preços
2. Treina um modelo RandomForest para prever preços de energia
3. Salva previsões no Data Lake (MinIO) como Parquet

Agendamento: Diário (@daily)
"""
import os
import logging
from datetime import datetime, timedelta

import numpy as np
import pandas as pd
import s3fs
import mlflow
import mlflow.sklearn
from sklearn.ensemble import RandomForestRegressor
from sklearn.model_selection import train_test_split
from sklearn.metrics import mean_squared_error, mean_absolute_error, r2_score

from airflow import DAG
from airflow.operators.python import PythonOperator
from airflow.utils.trigger_rule import TriggerRule

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
DATALAKE_HISTORICAL_PATH = "s3://datalake/historical/"


def _get_s3fs():
    return s3fs.S3FileSystem(
        client_kwargs={"endpoint_url": MINIO_ENDPOINT},
        key=MINIO_ACCESS_KEY,
        secret=MINIO_SECRET_KEY,
    )


def extract_simulate_data(**kwargs):
    """Simula extração de dados históricos de chuva, demanda e preços da energia."""
    np.random.seed(42)
    n_samples = 1000

    data = []
    for submarket_id, sub_name in SUBMARKETS.items():
        rainfall = np.random.uniform(0, 100, n_samples)
        demand = np.random.uniform(5000, 15000, n_samples)

        base_price_factor = 200 + (demand * 0.01) - (rainfall * 1.5)

        if submarket_id == 0:
            base_price_factor += 50
        elif submarket_id == 1:
            base_price_factor -= 20
        elif submarket_id == 2:
            base_price_factor -= 50
        elif submarket_id == 3:
            base_price_factor -= 80

        prices = base_price_factor + np.random.normal(0, 10, n_samples)
        prices = np.clip(prices, 50, 500)

        df = pd.DataFrame(
            {
                "submarket": submarket_id,
                "rainfall": rainfall,
                "demand": demand,
                "price": prices,
            }
        )
        data.append(df)

    full_df = pd.concat(data, ignore_index=True)

    # Save historical data to Data Lake for audit trail
    try:
        fs = _get_s3fs()
        ts = datetime.now().strftime("%Y%m%d_%H%M%S")
        s3_path = f"s3://datalake/historical/snapshot_{ts}.parquet"
        try:
            fs.mkdir("datalake/historical")
        except Exception:
            pass
        with fs.open(s3_path, "wb") as f:
            full_df.to_parquet(f, engine="pyarrow")
        logger.info(f"Historical snapshot saved to {s3_path}")
    except Exception as e:
        logger.warning(f"Could not persist historical snapshot: {e}")

    # Also save locally for the training step
    full_df.to_csv("/tmp/historical_data.csv", index=False)
    logger.info(f"Data extraction complete. {len(full_df)} rows generated.")
    kwargs["ti"].xcom_push(key="row_count", value=len(full_df))


def train_model(**kwargs):
    """Treina o modelo RandomForest e registra no MLflow."""
    df = pd.read_csv("/tmp/historical_data.csv")

    X = df[["submarket", "rainfall", "demand"]]
    y = df["price"]

    X_train, X_test, y_train, y_test = train_test_split(
        X, y, test_size=0.2, random_state=42
    )

    mlflow.set_tracking_uri(MLFLOW_TRACKING_URI)
    mlflow.set_experiment("pluvia_price_forecaster")

    with mlflow.start_run(run_name=f"rf_daily_{datetime.now().strftime('%Y%m%d')}"):
        model = RandomForestRegressor(n_estimators=100, max_depth=8, random_state=42)
        model.fit(X_train, y_train)

        predictions = model.predict(X_test)
        mse = mean_squared_error(y_test, predictions)
        mae = mean_absolute_error(y_test, predictions)
        r2 = r2_score(y_test, predictions)

        logger.info(f"Model trained. MSE={mse:.4f} MAE={mae:.4f} R2={r2:.4f}")

        mlflow.log_param("n_estimators", 100)
        mlflow.log_param("max_depth", 8)
        mlflow.log_param("model_type", "RandomForestRegressor")
        mlflow.log_param("train_samples", len(X_train))
        mlflow.log_param("test_samples", len(X_test))

        mlflow.log_metric("mse", mse)
        mlflow.log_metric("mae", mae)
        mlflow.log_metric("r2", r2)

        # Log feature importances
        for i, col in enumerate(X.columns):
            mlflow.log_metric(f"feature_importance_{col}", model.feature_importances_[i])

        mlflow.sklearn.log_model(model, "random_forest_model")

        import joblib

        joblib.dump(model, "/tmp/latest_model.pkl")
        logger.info("Model saved to /tmp/latest_model.pkl")

        kwargs["ti"].xcom_push(key="mse", value=mse)
        kwargs["ti"].xcom_push(key="r2", value=r2)


def predict_and_load_datalake(**kwargs):
    """Gera previsões para hoje e salva como Parquet no Data Lake."""
    import joblib

    model = joblib.load("/tmp/latest_model.pkl")

    np.random.seed(int(datetime.now().timestamp()))

    today_predictions = []
    for submarket_id, sub_name in SUBMARKETS.items():
        current_rain = np.random.uniform(10, 90)
        current_demand = np.random.uniform(7000, 13000)

        X_today = pd.DataFrame(
            {
                "submarket": [submarket_id],
                "rainfall": [current_rain],
                "demand": [current_demand],
            }
        )

        predicted_price = float(model.predict(X_today)[0])
        volatility = np.random.uniform(0.10, 0.35)

        today_predictions.append(
            {
                "submarket_id": submarket_id,
                "submarket_name": sub_name,
                "base_price": round(predicted_price, 2),
                "volatility": round(volatility, 4),
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
    except Exception:
        pass

    with fs.open(DATALAKE_PRICES_PATH, "wb") as f:
        results_df.to_parquet(f, engine="pyarrow")

    logger.info(f"Prices saved to Data Lake at {DATALAKE_PRICES_PATH}")
    logger.info(results_df.to_string())

    kwargs["ti"].xcom_push(key="predictions", value=today_predictions)


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
    description="Pluvia: treina modelo de previsão de preços e publica no Data Lake",
    schedule_interval="@daily",
    catchup=False,
    tags=["mlops", "pluvia", "pricing"],
) as dag:

    t1 = PythonOperator(
        task_id="extract_simulate_data",
        python_callable=extract_simulate_data,
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
