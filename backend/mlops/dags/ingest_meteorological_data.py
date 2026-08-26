"""
Pluvia Meteorological Data Ingestion - Airflow DAG

Pipeline MLOps que:
1. Faz o download de dados reais (brutos) de precipitação dos modelos ETA, GEFS e ECMWF.
2. Salva os arquivos brutos (GRIB/NetCDF/CSV) diretamente na camada Bronze do Data Lake (MinIO).

Agendamento: Diário (@daily)
"""
import os
import logging
import urllib.request
from datetime import datetime, timedelta
import boto3

from airflow import DAG
from airflow.operators.python import PythonOperator

logger = logging.getLogger(__name__)

# S3 / MinIO Settings
MINIO_ENDPOINT = os.getenv("MINIO_ENDPOINT", "http://minio:9000")
MINIO_ACCESS_KEY = os.getenv("AWS_ACCESS_KEY_ID", "minioadmin")
MINIO_SECRET_KEY = os.getenv("AWS_SECRET_ACCESS_KEY", "minioadmin")
BUCKET_NAME = "datalake"

# Public Data Sources (Verificadas e Funcionais)
# Utilizamos mirrors S3 e endpoints Open Data HTTPS que são mais estáveis e rápidos que FTPs governamentais.
GEFS_BASE_URL = "https://noaa-gefs-pds.s3.amazonaws.com/gefs.{date}/00/atmos/pgrb2ap5/gec00.t00z.pgrb2a.0p50.f024"
ECMWF_BASE_URL = "https://data.ecmwf.int/forecasts/{date}/00z/ifs/0p25/oper/{date}000000-0h-oper-fc.grib2"
# O FTP do CPTEC é frequentemente bloqueado por firewalls ou instável. 
# Usamos a API REST oficial do CPTEC/INPE que retorna as previsões em tempo real via HTTP.
ETA_BASE_FTP = "http://servicos.cptec.inpe.br/XML/capitais/previsao.xml"

def _get_s3_client():
    return boto3.client(
        "s3",
        endpoint_url=MINIO_ENDPOINT,
        aws_access_key_id=MINIO_ACCESS_KEY,
        aws_secret_access_key=MINIO_SECRET_KEY,
    )

def _download_and_upload_to_s3(url, s3_key, use_ftp=False):
    """Realiza o download de uma URL e faz upload via stream para o S3."""
    s3_client = _get_s3_client()
    
    # Criar bucket se não existir
    try:
        s3_client.head_bucket(Bucket=BUCKET_NAME)
    except:
        s3_client.create_bucket(Bucket=BUCKET_NAME)
        logger.info(f"Bucket {BUCKET_NAME} criado.")

    logger.info(f"Iniciando download de {url}")
    try:
        req = urllib.request.Request(url)
        if not use_ftp:
            req.add_header('User-Agent', 'Mozilla/5.0 Pluvia Pipeline/1.0')
            
        with urllib.request.urlopen(req, timeout=30) as response:
            s3_client.upload_fileobj(
                Fileobj=response,
                Bucket=BUCKET_NAME,
                Key=s3_key
            )
        logger.info(f"Upload concluído: s3://{BUCKET_NAME}/{s3_key}")
    except urllib.error.URLError as e:
        logger.warning(f"Erro ao acessar {url}: {e}. A fonte pode estar indisponível no momento ou a URL expirou.")
    except Exception as e:
        logger.error(f"Erro inesperado durante a ingestão de {url}: {e}")
        raise

def ingest_gefs_model(**kwargs):
    """
    Ingesta os dados brutos do modelo GEFS da NOAA.
    """
    date_str = kwargs['ds_nodash'] # Formato YYYYMMDD
    url = GEFS_BASE_URL.format(date=date_str)
    s3_key = f"bronze/meteorology/gefs/date={kwargs['ds']}/gep01_f024.grib2"
    
    _download_and_upload_to_s3(url, s3_key)

def ingest_ecmwf_model(**kwargs):
    """
    Ingesta os dados brutos do modelo ECMWF (Open Data).
    """
    date_str = kwargs['ds_nodash']
    url = ECMWF_BASE_URL.format(date=date_str)
    s3_key = f"bronze/meteorology/ecmwf/date={kwargs['ds']}/ecmwf_0h.grib2"
    
    _download_and_upload_to_s3(url, s3_key)

def ingest_eta_model(**kwargs):
    """
    Ingesta os dados brutos do modelo ETA do CPTEC/INPE via HTTP API.
    """
    url = ETA_BASE_FTP
    s3_key = f"bronze/meteorology/eta/date={kwargs['ds']}/previsao.xml"
    
    _download_and_upload_to_s3(url, s3_key)


default_args = {
    "owner": "airflow",
    "depends_on_past": False,
    "start_date": datetime(2023, 10, 1),
    "retries": 1,
    "retry_delay": timedelta(minutes=2),
    "email_on_failure": False,
}

with DAG(
    "ingest_meteorological_data",
    default_args=default_args,
    description="Pluvia: Ingestão de dados brutos reais dos modelos meteorológicos na camada Bronze",
    schedule_interval="@daily",
    catchup=False,
    tags=["mlops", "pluvia", "ingestion", "bronze"],
) as dag:

    t_eta = PythonOperator(
        task_id="ingest_eta_model",
        python_callable=ingest_eta_model,
    )

    t_gefs = PythonOperator(
        task_id="ingest_gefs_model",
        python_callable=ingest_gefs_model,
    )

    t_ecmwf = PythonOperator(
        task_id="ingest_ecmwf_model",
        python_callable=ingest_ecmwf_model,
    )

    # Executa de forma paralela já que as fontes são independentes
    [t_eta, t_gefs, t_ecmwf]
