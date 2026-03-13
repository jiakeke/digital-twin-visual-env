from fastapi import APIRouter

from app.models.schemas import SourceStatus, SourcesResponse
from app.scheduler import source_status

router = APIRouter()


@router.get("/sources", response_model=SourcesResponse)
async def get_sources():
    statuses = [
        SourceStatus(
            name=name,
            enabled=info["enabled"],
            last_fetch_at=info["last_fetch_at"],
            last_error=info["last_error"],
        )
        for name, info in source_status.items()
    ]
    return SourcesResponse(sources=statuses)
