import os
import pandas as pd
import numpy as np
from datetime import datetime
import logging
import math

logger = logging.getLogger(__name__)

def norm_cdf(x: float) -> float:
    return (1.0 + math.erf(x / math.sqrt(2.0))) / 2.0

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
    def calculate_mtm(cls, contract_price: float, volume_mw: float, contract_type: int | str, submarket: int, start_date: datetime, end_date: datetime, strike_price: float | None = None) -> float:
        """
        Calcula o Mark-to-Market (MtM) do contrato.
        Tipos de Contrato:
        1 / 'Purchase': COMPRA (Long Forward)
        2 / 'Sale': VENDA (Short Forward)
        3 / 'Swap': Swap de Preço
        4 / 'OptionCall': Opção de Compra (Black-Scholes)
        5 / 'OptionPut': Opção de Venda (Black-Scholes)
        """
        curve = cls.generate_forward_curve(submarket, start_date, end_date)
        daily_volume = volume_mw * 24
        
        # Converte para int se vier como string
        type_map = {'Purchase': 1, 'Sale': 2, 'Swap': 3, 'OptionCall': 4, 'OptionPut': 5}
        if isinstance(contract_type, str):
            c_type = type_map.get(contract_type, 1)
        else:
            c_type = contract_type
            
        config = cls.SUBMARKET_CONFIGS.get(submarket, cls.SUBMARKET_CONFIGS[0])
        sigma = config["volatility"]
        
        daily_mtm = []
        # Para Black-Scholes simplificado, time to maturity em anos para cada dia da curva
        # Assumiremos T como o tempo do dia atual até o final do contrato.
        total_days = (end_date - start_date).days
        if total_days <= 0: total_days = 1
        
        for i, row in curve.iterrows():
            S = row["MarketPrice"]
            T = (total_days - i) / 365.0
            if T <= 0: T = 1/365.0 # Previne divisão por zero
            
            if c_type == 1 or c_type == 3: # Purchase ou Swap (Long)
                daily_mtm.append((S - contract_price) * daily_volume)
            elif c_type == 2: # Sale (Short)
                daily_mtm.append((contract_price - S) * daily_volume)
            elif c_type == 4: # OptionCall (Black-Scholes)
                K = strike_price if strike_price else contract_price
                r = 0.10 # Taxa livre de risco simplificada 10%
                d1 = (math.log(S / K) + (r + 0.5 * sigma**2) * T) / (sigma * math.sqrt(T))
                d2 = d1 - sigma * math.sqrt(T)
                call_price = S * norm_cdf(d1) - K * math.exp(-r * T) * norm_cdf(d2)
                daily_mtm.append(call_price * daily_volume)
            elif c_type == 5: # OptionPut (Black-Scholes)
                K = strike_price if strike_price else contract_price
                r = 0.10
                d1 = (math.log(S / K) + (r + 0.5 * sigma**2) * T) / (sigma * math.sqrt(T))
                d2 = d1 - sigma * math.sqrt(T)
                put_price = K * math.exp(-r * T) * norm_cdf(-d2) - S * norm_cdf(-d1)
                daily_mtm.append(put_price * daily_volume)
            else:
                daily_mtm.append(0.0)
                
        return float(sum(daily_mtm))
        
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
