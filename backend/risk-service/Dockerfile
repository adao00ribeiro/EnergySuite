FROM python:3.11-slim

WORKDIR /app

# Install system dependencies (e.g. for numpy/pandas compiling if needed)
RUN apt-get update && apt-get install -y \
    build-essential \
    && rm -rf /var/lib/apt/lists/*

# Configure pip timeout and retries for large packages (e.g. pyarrow, pandas)
ENV PIP_DEFAULT_TIMEOUT=100 \
    PIP_DISABLE_PIP_VERSION_CHECK=1

# Copy requirements and install
COPY requirements.txt .
RUN --mount=type=cache,target=/root/.cache/pip pip install --default-timeout=100 --retries 5 -r requirements.txt

# Copy application code
COPY src/ /app/src/

EXPOSE 8000

# Run FastAPI using uvicorn
CMD ["uvicorn", "src.main:app", "--host", "0.0.0.0", "--port", "8000"]
