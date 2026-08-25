import os
import pandas as pd
import numpy as np
from datetime import datetime
import logging

logger = logging.getLogger(__name__)

class RiskEngine:
    # Submarket configurations: Base Price (R$/MWh) and Volatility (std dev)
    # Fallback defaults used only when Data Lake is unavailable
    DEFAULT_SUBMARKET_CONFIGS = {
        0: {"name": "SUD", "base_price": 250.0, "volatility": 0.15},
        1: {"name": "SUL", "base_price": 200.0, "volatility": 0.20},
        2: {"name": "NE",  "base_price": 150.0, "volatility": 0.35},
        3: {"name": "N",   "base_price": 100.0, "volatility": 0.30},
    }

    SUBMARKET_CONFIGS = dict(DEFAULT_SUBMARKET_CONFIGS)

    MINIO_ENDPOINT = os.getenv("MINIO_ENDPOINT", "http://minio:9000")
    MINIO_ACCESS_KEY = os.getenv("MINIO_ACCESS_KEY", "minioadmin")
    MINIO_SECRET_KEY = os.getenv("MINIO_SECRET_KEY", "minioadmin")
    MINIO_BUCKET = os.getenv("MINIO_BUCKET", "datalake")

    _configs_loaded = False

    @classmethod
    def load_configs_from_datalake(cls, force: bool = False):
        if cls._configs_loaded and not force:
            return

        import s3fs

        s3_path = f"s3://{cls.MINIO_BUCKET}/prices/latest_prices.parquet"

        try:
            fs = s3fs.S3FileSystem(
                client_kwargs={'endpoint_url': cls.MINIO_ENDPOINT},
                key=cls.MINIO_ACCESS_KEY,
                secret=cls.MINIO_SECRET_KEY
            )
            if fs.exists(s3_path):
                with fs.open(s3_path, 'rb') as f:
                    df = pd.read_parquet(f)

                for _, row in df.iterrows():
                    sub_id = int(row['submarket_id'])
                    cls.SUBMARKET_CONFIGS[sub_id] = {
                        "name": row['submarket_name'],
                        "base_price": float(row['base_price']),
                        "volatility": float(row['volatility'])
                    }
                cls._configs_loaded = True
                logger.info("RiskEngine loaded dynamic configs from Data Lake.")
            else:
                logger.info("Data Lake parquet not found yet. Using default configs.")
        except Exception as e:
            logger.warning(f"Failed to load configs from Data Lake: {e}. Using defaults.")

    @classmethod
    def generate_forward_curve(cls, submarket: int, start_date: datetime, end_date: datetime) -> pd.DataFrame:
        """
        Gera uma curva forward estocástica de preços de energia (PLD simulado)
        usando um Geometric Brownian Motion (GBM) simplificado.
        """
        # Tratar casos onde startDate == endDate
        if start_date >= end_date:
            dates = pd.date_range(start=start_date, periods=1, freq='D')
        else:
            dates = pd.date_range(start=start_date, end=end_date, freq='D')
            
        n_days = len(dates)
            
        cls.load_configs_from_datalake()
        
        config = cls.SUBMARKET_CONFIGS.get(submarket, cls.SUBMARKET_CONFIGS[0])
        S0 = config["base_price"]
        sigma = config["volatility"]
        
        # Simulação estocástica vetorial (Random Walk)
        # Assumindo drift = 0
        dt = 1/365.0
        random_shocks = np.random.normal(0, np.sqrt(dt), n_days)
        price_path = np.zeros(n_days)
        price_path[0] = S0
        
        for t in range(1, n_days):
            price_path[t] = price_path[t-1] * np.exp(-0.5 * sigma**2 * dt + sigma * random_shocks[t])
            
        curve = pd.DataFrame({
            "Date": dates,
            "MarketPrice": price_path
        })
        
        return curve

    @classmethod
    def calculate_mtm(cls, contract_price: float, volume_mw: float, contract_type: int, submarket: int, start_date: datetime, end_date: datetime) -> float:
        """
        Calcula o Mark-to-Market (MtM) do contrato.
        Type 0: COMPRA (Long) -> Ganha se Preço de Mercado > Preço do Contrato
        Type 1: VENDA (Short) -> Ganha se Preço de Mercado < Preço do Contrato
        """
        # 1 MWm significa 1 MW gerado em todas as horas do mês/período.
        # Aproximação: volume_mw * 24 horas * dias.
        
        curve = cls.generate_forward_curve(submarket, start_date, end_date)
        
        # Para cada dia, o volume diário é volume_mw * 24
        daily_volume = volume_mw * 24
        
        # Vetorização do cálculo diário de MtM
        # Se for COMPRA (0), pago fixo e recebo spot: (MarketPrice - ContractPrice) * Volume
        # Se for VENDA (1), recebo fixo e pago spot: (ContractPrice - MarketPrice) * Volume
        
        if contract_type == 0:
            curve["DailyMtM"] = (curve["MarketPrice"] - contract_price) * daily_volume
        else:
            curve["DailyMtM"] = (contract_price - curve["MarketPrice"]) * daily_volume
            
        total_mtm = float(curve["DailyMtM"].sum())
        return total_mtm
        
    @classmethod
    def determine_risk_category(cls, mtm: float) -> str:
        """
        Classifica o risco baseado na expectativa de prejuízo ou lucro.
        """
        if mtm < -1_000_000:
            return "HIGH"
        elif mtm < 0:
            return "MEDIUM"
        else:
            return "LOW"
