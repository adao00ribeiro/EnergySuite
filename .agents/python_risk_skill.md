# Diretrizes para o Agente: Desenvolvimento Científico e Risco (Python)

Você está atuando nos módulos analíticos da Suite for Energy (Imeris para Risco, Pluvia para ML).
Seu papel principal é desenvolver engines matemáticos de alta performance.

## 1. Framework Base
- Use **FastAPI** para expor endpoints REST ou webhooks caso o sistema precise ser chamado sincronicamente.
- Todo endpoint deve ter tipagem estrita com **Pydantic**. O código deve passar perfeitamente em checagens do `mypy`.

## 2. Processamento Matemático (Performance)
- NUNCA use loops `for` em Python padrão se você puder vetorizar a operação.
- Toda matriz de cenário estocástico deve ser processada via **NumPy** ou **Pandas**.
- Exemplo: Ao invés de iterar sobre 2.000 cenários calculando o retorno de um contrato por dia, faça o broadcast matricial em NumPy. Se o volume de dados não couber em RAM, opte por `Dask` ou `Polars`.

## 3. Event-Driven & Kafka
- A engine Python deve ser um consumidor (Consumer) ativo dos eventos gerados pelo .NET ETRM (ex: Assinatura de novo contrato).
- Use `confluent-kafka-python` ou `aiokafka` (assíncrono) para escutar eventos, recalcular os dados (ex: VaR) em background e salvar no banco de dados analítico.

## 4. Integração com Data Lakehouse (S3 / Parquet)
- Arquivos massivos resultantes das simulações devem ser salvos em formato **Parquet** (preferencialmente particionado por data e modelo). Evite CSV a todo custo.
- Use `pyarrow` ou `fastparquet` para interagir com o Data Lake.

## 5. MLOps e Rastreabilidade
- Todo experimento de Machine Learning (Pluvia) deve registrar parâmetros, métricas e o próprio modelo treinado utilizando **MLflow**.
