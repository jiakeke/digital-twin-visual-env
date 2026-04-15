import logging
from datetime import datetime, timezone

from apscheduler.schedulers.asyncio import AsyncIOScheduler

from app.cache import set_latest
from app.collectors.fmi import FMICollector
from app.collectors.hsy import HSYCollector
from app.collectors.team_b import TeamBCollector
from app.config import settings
from app.database import AsyncSessionLocal
from app.models.db import SensorReading

logger = logging.getLogger(__name__)

scheduler = AsyncIOScheduler()

# TTLs per source (seconds)
_TTL = {
    "fmi": 15 * 60,   # 15 minutes
    "hsy": 90 * 60,   # 90 minutes
    "team_b": 5 * 60, # 5 minutes (TBD)
}

# Source status registry (in-memory, visible via /api/v1/sources)
source_status: dict[str, dict] = {
    "fmi": {"enabled": settings.fmi_enabled, "last_fetch_at": None, "last_error": None},
    "hsy": {"enabled": settings.hsy_enabled, "last_fetch_at": None, "last_error": None},
    "team_b": {"enabled": settings.team_b_enabled, "last_fetch_at": None, "last_error": None},
}


async def _run_collector(collector) -> None:
    name = collector.source_name
    try:
        readings = await collector.run()
        async with AsyncSessionLocal() as session:
            for r in readings:
                row = SensorReading(
                    source=r.source,
                    location_id=r.location_id,
                    latitude=r.latitude,
                    longitude=r.longitude,
                    measured_at=r.measured_at,
                    fetched_at=datetime.now(timezone.utc),
                    metric=r.metric,
                    value=r.value,
                    unit=r.unit,
                )
                session.add(row)
                await set_latest(
                    r.source, r.metric, r.location_id,
                    r.model_dump(mode="json"),
                    ttl=_TTL.get(name, 900),
                )
            await session.commit()
        source_status[name]["last_fetch_at"] = datetime.now(timezone.utc).isoformat()
        source_status[name]["last_error"] = None
        logger.info("Collector '%s' completed: %d readings", name, len(readings))
    except Exception as exc:
        source_status[name]["last_error"] = str(exc)
        logger.error("Collector '%s' failed: %s", name, exc)


def start_scheduler() -> None:
    if settings.fmi_enabled:
        scheduler.add_job(
            _run_collector,
            "interval",
            minutes=settings.fmi_poll_interval_minutes,
            args=[FMICollector()],
            id="fmi",
            next_run_time=datetime.now(timezone.utc),
        )
    if settings.hsy_enabled:
        scheduler.add_job(
            _run_collector,
            "interval",
            minutes=settings.hsy_poll_interval_minutes,
            args=[HSYCollector()],
            id="hsy",
            next_run_time=datetime.now(timezone.utc),
        )
    if settings.team_b_enabled:
        scheduler.add_job(
            _run_collector,
            "interval",
            minutes=settings.team_b_poll_interval_minutes,
            args=[TeamBCollector()],
            id="team_b",
            next_run_time=datetime.now(timezone.utc),
        )
    scheduler.start()
    logger.info("Scheduler started.")


def stop_scheduler() -> None:
    scheduler.shutdown(wait=False)
