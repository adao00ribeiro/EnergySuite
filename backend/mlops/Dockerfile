FROM apache/airflow:2.7.0-python3.11

ENV PIP_DEFAULT_TIMEOUT=100

COPY requirements.txt .
RUN --mount=type=cache,target=/home/airflow/.cache/pip pip install --default-timeout=100 --retries 5 -r requirements.txt
