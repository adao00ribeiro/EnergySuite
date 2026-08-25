SELECT 'CREATE DATABASE risk_db'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'risk_db')\gexec
