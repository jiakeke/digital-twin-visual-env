import httpx

from app.collectors.base import BaseCollector
from app.config import settings
from app.models.schemas import NormalizedReading

# FMI WFS stored query for weather observations
_STORED_QUERY = "fmi::observations::weather::multipointcoverage"


class FMICollector(BaseCollector):
    source_name = "fmi"
    enabled = settings.fmi_enabled

    async def fetch(self) -> dict:
        params = {
            "service": "WFS",
            "version": "2.0.0",
            "request": "getFeature",
            "storedquery_id": _STORED_QUERY,
            "place": settings.fmi_location,
        }
        async with httpx.AsyncClient(timeout=30) as client:
            resp = await client.get(settings.fmi_wfs_url, params=params)
            resp.raise_for_status()
        return {"xml": resp.text}

    def normalize(self, raw: dict) -> list[NormalizedReading]:
        # TODO: parse FMI WFS XML/GML (multipointcoverage format)
        # Use xmltodict or lxml to extract stations and readings.
        # Each station will have: location (lat/lon WGS84), metrics list.
        raise NotImplementedError("FMI XML parsing not yet implemented")
