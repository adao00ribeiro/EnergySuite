import uuid
from datetime import datetime
from sqlalchemy import Column, String, Float, DateTime
from sqlalchemy.dialects.postgresql import UUID
from pydantic import BaseModel
from .database import Base

# SQLAlchemy Database Model
class RiskMetricModel(Base):
    __tablename__ = "risk_metrics"

    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    contract_id = Column(UUID(as_uuid=True), unique=True, nullable=False)
    counterparty_name = Column(String(200), nullable=False)
    financial_exposure = Column(Float, nullable=False)
    risk_category = Column(String(50), nullable=False)
    calculated_at = Column(DateTime, default=datetime.utcnow)

# Pydantic Schemas for Kafka Event Payload
class ContractCreatedEvent(BaseModel):
    contractId: uuid.UUID
    counterpartyName: str
    type: str
    submarket: str
    volumeMwMed: float
    price: float
    startDate: datetime
    endDate: datetime
    createdAt: datetime

# Pydantic Schemas for API Response
class RiskMetricResponse(BaseModel):
    id: uuid.UUID
    contract_id: uuid.UUID
    counterparty_name: str
    financial_exposure: float
    risk_category: str
    calculated_at: datetime

    class Config:
        orm_mode = True
        from_attributes = True

# Pydantic Schema for Event Publication (BI)
class RiskCalculatedEvent(BaseModel):
    eventId: uuid.UUID
    contractId: uuid.UUID
    counterpartyName: str
    financialExposure: float
    riskCategory: str
    calculatedAt: datetime
