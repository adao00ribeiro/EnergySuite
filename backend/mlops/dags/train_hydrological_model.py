import os
from datetime import datetime, timedelta
import random

from airflow import DAG
from airflow.operators.python import PythonOperator
import mlflow

# Configuration
MLFLOW_TRACKING_URI = os.getenv("MLFLOW_TRACKING_URI", "http://mlflow:5000")
S3_ENDPOINT_URL = os.getenv("MINIO_ENDPOINT", "http://minio:9000")
os.environ["MLFLOW_S3_ENDPOINT_URL"] = S3_ENDPOINT_URL

default_args = {
    'owner': 'pluvia',
    'depends_on_past': False,
    'start_date': datetime(2023, 10, 1),
    'email_on_failure': False,
    'email_on_retry': False,
    'retries': 1,
    'retry_delay': timedelta(minutes=5),
}

def train_smap_model_task(**kwargs):
    """
    Simulates training a hydrological model (SMAP or Neural Net)
    and logs the metrics/parameters to MLflow.
    """
    mlflow.set_tracking_uri(MLFLOW_TRACKING_URI)
    mlflow.set_experiment("Pluvia_Hydrological_SMAP")

    # Mock parameters
    learning_rate = random.uniform(0.001, 0.05)
    epochs = random.randint(50, 200)
    
    # Mock metrics
    initial_rmse = 50.0
    final_rmse = initial_rmse - random.uniform(10.0, 30.0)
    mae = final_rmse * 0.8
    
    with mlflow.start_run(run_name=f"SMAP_Train_{datetime.now().strftime('%Y%m%d%H%M%S')}"):
        mlflow.log_param("learning_rate", learning_rate)
        mlflow.log_param("epochs", epochs)
        mlflow.log_param("model_type", "SMAP_NeuralNet")
        
        # Simulating training loop
        print(f"Training SMAP model for {epochs} epochs with lr={learning_rate}")
        for epoch in range(1, epochs + 1):
            current_rmse = initial_rmse - ((initial_rmse - final_rmse) * (epoch / epochs))
            if epoch % 10 == 0:
                mlflow.log_metric("rmse", current_rmse, step=epoch)
        
        mlflow.log_metric("final_rmse", final_rmse)
        mlflow.log_metric("mae", mae)
        
        # In a real scenario, we would log the model artifact using:
        # mlflow.sklearn.log_model(sk_model=model, artifact_path="model")
        
        print(f"Training finished. Final RMSE: {final_rmse:.2f}, MAE: {mae:.2f}")

with DAG(
    'train_hydrological_model',
    default_args=default_args,
    description='Treinamento e versionamento (MLflow) do motor hidrológico SMAP',
    schedule_interval=timedelta(days=7), # Train weekly
    catchup=False,
) as dag:

    train_model = PythonOperator(
        task_id='train_smap_model',
        python_callable=train_smap_model_task,
        provide_context=True,
    )

    train_model
