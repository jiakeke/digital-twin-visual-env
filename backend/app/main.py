from contextlib import asynccontextmanager

from fastapi import FastAPI

from app.api.v1.router import router
from app.cache import close_redis
from app.database import engine
from app.models.db import Base
from app.scheduler import start_scheduler, stop_scheduler


@asynccontextmanager
async def lifespan(app: FastAPI):
    # Startup: create tables and start scheduler
    async with engine.begin() as conn:
        await conn.run_sync(Base.metadata.create_all)
    start_scheduler()
    yield
    # Shutdown
    stop_scheduler()
    await close_redis()
    await engine.dispose()


app = FastAPI(
    title="Digital Twin Environmental Data API",
    description="Aggregates environmental data from FMI, HSY, and Team B sensors for the Helsinki Digital Twin.",
    version="0.1.0",
    lifespan=lifespan,
)

app.include_router(router)
