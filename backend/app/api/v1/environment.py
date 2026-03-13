from datetime import datetime, timezone

from fastapi import APIRouter, Depends, Query
from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession

from app.cache import get_latest
from app.database import get_db
from app.models.db import SensorReading
from app.models.schemas import LatestResponse, NormalizedReading

router = APIRouter()


@router.get("/environment/latest", response_model=LatestResponse)
async def get_latest_readings(
    source: str | None = Query(None),
    metric: str | None = Query(None),
    db: AsyncSession = Depends(get_db),
):
    cached = await get_latest(source=source, metric=metric)
    if cached:
        readings = [NormalizedReading(**r) for r in cached]
    else:
        # Fallback to PostgreSQL if Redis is empty
        stmt = select(SensorReading)
        if source:
            stmt = stmt.where(SensorReading.source == source)
        if metric:
            stmt = stmt.where(SensorReading.metric == metric)
        stmt = stmt.order_by(SensorReading.measured_at.desc()).limit(500)
        result = await db.execute(stmt)
        rows = result.scalars().all()
        readings = [
            NormalizedReading(
                source=r.source,
                location_id=r.location_id,
                latitude=r.latitude,
                longitude=r.longitude,
                measured_at=r.measured_at,
                metric=r.metric,
                value=r.value,
                unit=r.unit,
            )
            for r in rows
        ]
    return LatestResponse(fetched_at=datetime.now(timezone.utc), readings=readings)


@router.get("/environment/history")
async def get_history(
    source: str = Query(...),
    metric: str = Query(...),
    location_id: str | None = Query(None),
    from_: datetime = Query(..., alias="from"),
    to: datetime = Query(...),
    db: AsyncSession = Depends(get_db),
):
    stmt = (
        select(SensorReading)
        .where(SensorReading.source == source)
        .where(SensorReading.metric == metric)
        .where(SensorReading.measured_at >= from_)
        .where(SensorReading.measured_at <= to)
        .order_by(SensorReading.measured_at.asc())
    )
    if location_id:
        stmt = stmt.where(SensorReading.location_id == location_id)

    result = await db.execute(stmt)
    rows = result.scalars().all()
    readings = [
        NormalizedReading(
            source=r.source,
            location_id=r.location_id,
            latitude=r.latitude,
            longitude=r.longitude,
            measured_at=r.measured_at,
            metric=r.metric,
            value=r.value,
            unit=r.unit,
        )
        for r in rows
    ]
    return {"from": from_, "to": to, "readings": readings}
