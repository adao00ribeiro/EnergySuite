import boto3
import os
import io
import uuid
import logging

logger = logging.getLogger(__name__)

class GevazpGenerator:
    def __init__(self):
        # Using the internal docker network endpoint for minio
        self.endpoint_url = os.getenv("MINIO_ENDPOINT", "http://minio:9000")
        self.s3 = boto3.client(
            's3',
            endpoint_url=self.endpoint_url,
            aws_access_key_id='minioadmin',
            aws_secret_access_key='minioadmin'
        )
        self.bucket = "datalake"

    def generate_and_upload(self, simulation_id: uuid.UUID):
        try:
            # Create dummy content representing ONS/CCEE standard txt files
            files_to_generate = {
                "PREVS.rv0": "XX PREVISAO DE VAZOES\n001 ITAIPU        12345 54321 65432\n002 FURNAS        98765 43210 23456\n",
                "ENA.rv0": "XX ENERGIAS NATURAIS AFLUENTES\nSE   25000 24000\nS    12000 11000\n",
                "VNA.rv0": "XX VOLUMES NATURAIS AFLUENTES\n001 0.5 0.6 0.7\n002 0.3 0.4 0.5\n",
                "DADVAZ.rv0": "XX DADOS DE VAZAO HISTORICA\n1931 1 123 124 125 126\n",
                "STR.rv0": "XX ESTRUTURA TOPOLOGICA\nFURNAS -> MASCARENHAS\n"
            }

            for filename, content in files_to_generate.items():
                object_key = f"exports/{simulation_id}/{filename}"
                
                # Upload to MinIO
                self.s3.put_object(
                    Bucket=self.bucket,
                    Key=object_key,
                    Body=content.encode('utf-8')
                )
                
            logger.info(f"GEVAZP files successfully uploaded for simulation {simulation_id} to S3 ({self.bucket}/exports)")
        except Exception as e:
            logger.error(f"Failed to generate and upload GEVAZP files: {e}")
