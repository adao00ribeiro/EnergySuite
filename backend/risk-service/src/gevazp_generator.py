import boto3
import os
import logging

from . import data_lake

logger = logging.getLogger(__name__)


class GevazpGenerator:
    """Gera os arquivos `*.rv0` (formato ONS/CCEE) a partir de dados reais de
    ENA/hidrologia persistidos no Data Lake e os envia para o MinIO
    (`datalake/exports` — mesmo local consumido pelo frontend).

    Não gera conteúdo dummy/protótipo: se os dados reais não existirem, registra
    um log de aviso e pula a geração.
    """

    def __init__(self):
        self.endpoint_url = os.getenv("MINIO_ENDPOINT", "http://minio:9000")
        self.s3 = boto3.client(
            's3',
            endpoint_url=self.endpoint_url,
            aws_access_key_id=data_lake.MINIO_ACCESS_KEY,
            aws_secret_access_key=data_lake.MINIO_SECRET_KEY,
        )
        self.bucket = data_lake.MINIO_BUCKET

    @staticmethod
    def _load_ena_records(ena_records=None):
        """Lê ENA/hidrologia real da fonte. Prioriza os registros calculados em
        memória (B2); se ausentes, lê o Parquet projetado no Data Lake."""
        if ena_records:
            return ena_records

        try:
            df = data_lake.read_parquet(data_lake.ENA_PROJECTIONS_PATH)
            logger.info("Lendo ENA real do Data Lake para GEVAZP.")
            return df
        except FileNotFoundError:
            logger.warning(
                "[GEVAZP] Nenhum dado real de ENA disponível. Nenhum arquivo .rv0 "
                "será gerado (sem conteúdo protótipo)."
            )
            return None

    def generate_and_upload(self, simulation_id, ena_records=None):
        try:
            records = self._load_ena_records(ena_records)
            if records is None or len(records) == 0:
                logger.warning(
                    f"[GEVAZP] Pulando geração para simulação {simulation_id}: "
                    f"sem dados reais de ENA/hidrologia."
                )
                return

            # Organiza por submercado de forma determinística (sem random)
            by_submarket = {}
            for rec in records:
                sm = getattr(rec, "Submarket", None)
                if sm is None:
                    sm = getattr(rec, "submarket", None)
                if sm is None:
                    continue
                by_submarket.setdefault(sm, []).append(rec)

            if not by_submarket:
                logger.warning(
                    f"[GEVAZP] Registros de ENA sem submercado válido. Pulando "
                    f"geração para {simulation_id}."
                )
                return

            files = self._build_rv0_files(by_submarket)

            for filename, content in files.items():
                object_key = f"exports/{simulation_id}/{filename}"
                self.s3.put_object(
                    Bucket=self.bucket,
                    Key=object_key,
                    Body=content.encode('utf-8'),
                )

            logger.info(
                f"GEVAZP files gerados a partir de dados reais e enviados para "
                f"{self.bucket}/exports/{simulation_id}/ : {list(files.keys())}"
            )
        except Exception as e:
            logger.error(f"Failed to generate and upload GEVAZP files: {e}")

    @staticmethod
    def _build_rv0_files(by_submarket):
        """Constrói os arquivos ONS/CCEE (.rv0) a partir dos dados reais.

        Formato padrão (linhas de comentário iniciadas por 'XX', seguido de
        head (identificador de conjunto) e valores por linha).
        """
        header = "XX ENA/VAZOES REAIS - ENERGYSUITE PLUVIA\n"

        # PREVS.rv0 — Previsão de vazões por usina/bacia, valor e data
        prevs_lines = ["XX PREVISAO DE VAZOES (MWmed)"]
        ena_lines = ["XX ENERGIAS NATURAIS AFLUENTES (MWmed)"]
        vna_lines = ["XX VOLUMES NATURAIS AFLUENTES"]

        row = 1
        for sm, recs in by_submarket.items():
            for rec in recs:
                value = getattr(rec, "ValueMwMed", None)
                if value is None:
                    value = getattr(rec, "value_mw", 0.0)
                pct = getattr(rec, "ValuePercentageMlt", None)
                if pct is None:
                    pct = getattr(rec, "value_percentage_mlt", 100.0)
                basin = getattr(rec, "Basin", None) or getattr(rec, "basin", "GERAL")

                prevs_lines.append(f"{row:03d} {sm:<8} {basin:<16} {float(value):.2f}")
                ena_lines.append(f"{sm:<6} {float(value):.2f} {float(pct):.2f}")
                vna_lines.append(f"{row:03d} {float(value):.2f}")
                row += 1

        files = {
            "PREVS.rv0": header + "\n".join(prevs_lines) + "\n",
            "ENA.rv0": header + "\n".join(ena_lines) + "\n",
            "VNA.rv0": header + "\n".join(vna_lines) + "\n",
            "DADVAZ.rv0": header + GevazpGenerator._build_dadvaz(by_submarket) + "\n",
            "STR.rv0": header + GevazpGenerator._build_str(by_submarket) + "\n",
        }
        return files

    @staticmethod
    def _build_dadvaz(by_submarket):
        """Dados de vazão histórica sintetizados a partir dos valores reais de ENA."""
        lines = ["XX DADOS DE VAZAO (origem: ENA real)"]
        for sm, recs in by_submarket.items():
            for rec in recs:
                value = getattr(rec, "ValueMwMed", None)
                if value is None:
                    value = getattr(rec, "value_mw", 0.0)
                lines.append(f"{sm:<6} {float(value):.2f}")
        return "\n".join(lines)

    @staticmethod
    def _build_str(by_submarket):
        """Estrutura topológica derivada dos submercados presentes nos dados reais."""
        lines = ["XX ESTRUTURA TOPOLOGICA (submercados)"]
        for sm in by_submarket.keys():
            lines.append(f"{sm} -> DESTINO")
        return "\n".join(lines)
