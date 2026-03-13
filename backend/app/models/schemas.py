from datetime import datetime
from pydantic import BaseModel


class NormalizedReading(BaseModel):
    source: str          # 'fmi', 'hsy', 'team_b'
    location_id: str     # station or area ID
    latitude: float      # WGS84
    longitude: float     # WGS84
    measured_at: datetime
    metric: str          # standardized metric name
    value: float
    unit: str


class LatestResponse(BaseModel):
    fetched_at: datetime
    readings: list[NormalizedReading]


class SourceStatus(BaseModel):
    name: str
    enabled: bool
    last_fetch_at: datetime | None = None
    last_error: str | None = None


class SourcesResponse(BaseModel):
    sources: list[SourceStatus]
