import os
import uuid
from datetime import datetime, timedelta
import pandas as pd
import numpy as np
import s3fs
import mlflow
import mlflow.sklearn
from sklearn.ensemble import RandomForestRegressor
from sklearn.model_selection import train_test_split
from sklearn.metrics import mean_squared_error

from airflow import DAG
from airflow.operators.python import PythonOperator

# S3 / MinIO Settings
MINIO_URL = "http://minio:9000"
os.environ["AWS_ACCESS_KEY_ID"] = "minioadmin"
os.environ["AWS_SECRET_ACCESS_KEY"] = "minioadmin"
os.environ["AWS_ENDPOINT_URL"] = MINIO_URL
MLFLOW_TRACKING_URI = "http://mlflow:5000"

SUBMARKETS = {
    0: "SE_CO",
    1: "SUL",
    2: "NE",
    3: "N"
}

def extract_simulate_data(**kwargs):
    """Simulates extraction of ONS climate and load data."""
    # Create a synthetic historical dataset for energy prices based on rain and load
    np.random.seed(42)
    n_samples = 1000
    
    data = []
    for submarket_id, sub_name in SUBMARKETS.items():
        # Rainfall index (0 to 100)
        rainfall = np.random.uniform(0, 100, n_samples)
        # Energy Load (Demand)
        demand = np.random.uniform(5000, 15000, n_samples)
        
        # Base price calculation (synthetic relationship)
        # More rain -> lower price. More demand -> higher price.
        base_price_factor = 200 + (demand * 0.01) - (rainfall * 1.5)
        
        # Submarket specific biases
        if submarket_id == 0: base_price_factor += 50
        elif submarket_id == 1: base_price_factor -= 20
        elif submarket_id == 2: base_price_factor -= 50
        elif submarket_id == 3: base_price_factor -= 80
            
        prices = base_price_factor + np.random.normal(0, 10, n_samples)
        prices = np.clip(prices, 50, 500) # Price caps
        
        df = pd.DataFrame({
            'submarket': submarket_id,
            'rainfall': rainfall,
            'demand': demand,
            'price': prices
        })
        data.append(df)
        
    full_df = pd.concat(data, ignore_index=True)
    full_df.to_csv('/tmp/historical_data.csv', index=False)
    print("Data extraction complete.")

def train_model(**kwargs):
    """Trains an ML model and logs to MLflow."""
    df = pd.read_csv('/tmp/historical_data.csv')
    
    X = df[['submarket', 'rainfall', 'demand']]
    y = df['price']
    
    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)
    
    mlflow.set_tracking_uri(MLFLOW_TRACKING_URI)
    mlflow.set_experiment("pluvia_price_forecaster")
    
    with mlflow.start_run():
        model = RandomForestRegressor(n_estimators=50, max_depth=5, random_state=42)
        model.fit(X_train, y_train)
        
        predictions = model.predict(X_test)
        mse = mean_squared_error(y_test, predictions)
        print(f"Model trained. MSE: {mse}")
        
        mlflow.log_param("n_estimators", 50)
        mlflow.log_param("max_depth", 5)
        mlflow.log_metric("mse", mse)
        mlflow.sklearn.log_model(model, "random_forest_model")
        
        # Save model locally for the next step
        import joblib
        joblib.dump(model, '/tmp/latest_model.pkl')

def predict_and_load_datalake(**kwargs):
    """Predicts today's prices and saves as Parquet in MinIO (Data Lake)."""
    import joblib
    model = joblib.load('/tmp/latest_model.pkl')
    
    # Simulate today's weather and demand
    np.random.seed(int(datetime.now().timestamp()))
    
    today_predictions = []
    for submarket_id, sub_name in SUBMARKETS.items():
        # Get random current rainfall and demand
        current_rain = np.random.uniform(10, 90)
        current_demand = np.random.uniform(7000, 13000)
        
        X_today = pd.DataFrame({
            'submarket': [submarket_id],
            'rainfall': [current_rain],
            'demand': [current_demand]
        })
        
        predicted_price = float(model.predict(X_today)[0])
        
        # Calculate volatility based on recent variance (synthetic)
        volatility = np.random.uniform(0.10, 0.35)
        
        today_predictions.append({
            'submarket_id': submarket_id,
            'submarket_name': sub_name,
            'base_price': predicted_price,
            'volatility': volatility,
            'date': datetime.now().date()
        })
        
    results_df = pd.DataFrame(today_predictions)
    
    # Save to MinIO Data Lake
    s3_path = "s3://datalake/prices/latest_prices.parquet"
    
    fs = s3fs.S3FileSystem(
        client_kwargs={'endpoint_url': MINIO_URL},
        key=os.environ["AWS_ACCESS_KEY_ID"],
        secret=os.environ["AWS_SECRET_ACCESS_KEY"]
    )
    
    # Ensure bucket exists (just in case the mc script failed, though it should exist)
    try:
        fs.mkdir('datalake')
    except Exception:
        pass
        
    with fs.open(s3_path, 'wb') as f:
        results_df.to_parquet(f, engine='pyarrow')
        
    print(f"Prices saved to Data Lake at {s3_path}")
    print(results_df)


default_args = {
    'owner': 'airflow',
    'depends_on_past': False,
    'start_date': datetime(2026, 1, 1),
    'retries': 1,
    'retry_delay': timedelta(minutes=5),
}

with DAG(
    'train_price_model',
    default_args=default_args,
    description='Train Pluvia pricing model and load to Data Lake',
    schedule_interval='@daily',
    catchup=False,
) as dag:

    t1 = PythonOperator(
        task_id='extract_simulate_data',
        python_callable=extract_simulate_data,
    )

    t2 = PythonOperator(
        task_id='train_model',
        python_callable=train_model,
    )

    t3 = PythonOperator(
        task_id='predict_and_load_datalake',
        python_callable=predict_and_load_datalake,
    )

    t1 >> t2 >> t3
