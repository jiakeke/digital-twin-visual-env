"""HSY (Helsinki Region Environmental Services) WFS collector.

Fetches current air quality index from the HSY GeoServer WFS endpoint.

Feature type: ilmanlaatu:Ilmanlaatu_nyt
- Returns 13 stations across the Helsinki metropolitan area
- Each feature has: Ilmanlaatuindeksi (AQI, Finnish index 0–100+),
  Mittausaseman_numero (station ID), Mittausasema (name), Aika (Finnish datetime)
- Geometry coordinates are in EPSG:3879 (Finnish local CRS) — converted to WGS84

Finnish datetime format example: "14.3.2026 klo 1" → hour-level granularity.
"""

from datetime import datetime, timezone

import httpx
from pyproj import Transformer

from app.collectors.base import BaseCollector
from app.config import settings
from app.models.schemas import NormalizedReading

_FEATURE_TYPE = "ilmanlaatu:Ilmanlaatu_nyt"

# Singleton transformer: EPSG:3879 → WGS84 (lon, lat order with always_xy=True)
_transformer = Transformer.from_crs("EPSG:3879", "EPSG:4326", always_xy=True)


def _parse_finnish_datetime(aika: str) -> datetime:
    """Parse HSY Finnish datetime string e.g. '14.3.2026 klo 1' → UTC datetime."""
    # Format: "D.M.YYYY klo H"
    date_part, _, time_part = aika.partition(" klo ")
    day, month, year = date_part.strip().split(".")
    hour = int(time_part.strip())
    return datetime(int(year), int(month), int(day), hour, 0, 0, tzinfo=timezone.utc)


class HSYCollector(BaseCollector):
    source_name = "hsy"
    enabled = settings.hsy_enabled

    async def fetch(self) -> dict:
        params = {
            "service": "WFS",
            "version": "2.0.0",
            "request": "GetFeature",
            "typeName": _FEATURE_TYPE,
            "outputFormat": "application/json",
        }
        async with httpx.AsyncClient(timeout=30) as client:
            resp = await client.get(settings.hsy_wfs_url, params=params)
            resp.raise_for_status()
        return resp.json()

    def normalize(self, raw: dict) -> list[NormalizedReading]:
        readings: list[NormalizedReading] = []

        for feature in raw.get("features", []):
            props = feature.get("properties", {})
            geometry = feature.get("geometry", {})

            station_num = props.get("Mittausaseman_numero")
            aqi = props.get("Ilmanlaatuindeksi")
            aika = props.get("Aika", "")
            coords = geometry.get("coordinates", [])

            if station_num is None or aqi is None or len(coords) < 2:
                continue

            # Convert EPSG:3879 → WGS84
            lon, lat = _transformer.transform(coords[0], coords[1])

            try:
                measured_at = _parse_finnish_datetime(aika)
            except (ValueError, AttributeError):
                measured_at = datetime.now(timezone.utc)

            readings.append(
                NormalizedReading(
                    source=self.source_name,
                    location_id=f"hsy_{station_num}",
                    latitude=lat,
                    longitude=lon,
                    measured_at=measured_at,
                    metric="aqi",
                    value=float(aqi),
                    unit="index",
                )
            )

        return readings
