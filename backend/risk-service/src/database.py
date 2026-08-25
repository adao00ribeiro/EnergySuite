import os
from sqlalchemy.ext.asyncio import create_async_engine, async_sessionmaker, AsyncSession
from sqlalchemy.orm import declarative_base

# The backend .NET service uses 'etrm_db' with user 'root' and pass 'rootpassword'
DATABASE_URL = os.getenv("DATABASE_URL", "postgresql+asyncpg://root:rootpassword@postgres:5432/etrm_db")

engine = create_async_engine(DATABASE_URL, echo=True)

AsyncSessionLocal = async_sessionmaker(
    bind=engine, class_=AsyncSession, expire_on_commit=False
)

Base = declarative_base()

async def get_db():
    async with AsyncSessionLocal() as session:
        yield session
