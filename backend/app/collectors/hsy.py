import httpx

from app.collectors.base import BaseCollector
from app.config import settings
from app.models.schemas import NormalizedReading


class HSYCollector(BaseCollector):
    source_name = "hsy"
    enabled = settings.hsy_enabled

    async def fetch(self) -> dict:
        params = {
            "service": "WFS",
            "version": "2.0.0",
            "request": "GetFeature",
            "typeName": "ilmanlaatu:Ilmanlaatupiste",
            "outputFormat": "application/json",
        }
        async with httpx.AsyncClient(timeout=30) as client:
            resp = await client.get(settings.hsy_wfs_url, params=params)
            resp.raise_for_status()
        return resp.json()

    def normalize(self, raw: dict) -> list[NormalizedReading]:
        # TODO: parse HSY GeoJSON response.
        # Coordinates are in EPSG:3879 — convert to WGS84 using pyproj:
        #   from pyproj import Transformer
        #   transformer = Transformer.from_crs("EPSG:3879", "EPSG:4326", always_xy=True)
        #   lon, lat = transformer.transform(x, y)
        raise NotImplementedError("HSY GeoJSON parsing not yet implemented")
