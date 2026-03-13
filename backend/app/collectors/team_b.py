import random
from datetime import datetime, timezone

from app.collectors.base import BaseCollector
from app.config import settings
from app.models.schemas import NormalizedReading

# Plausible ranges for Helsinki sensor readings (mock data)
_MOCK_METRICS = [
    ("temperature", -5.0, 25.0, "°C"),
    ("humidity", 40.0, 95.0, "%"),
    ("pm25", 1.0, 35.0, "µg/m³"),
]

# Approximate Helsinki city center coordinates
_MOCK_LOCATION = {"location_id": "team_b_mock_01", "latitude": 60.1699, "longitude": 24.9384}


class TeamBCollector(BaseCollector):
    source_name = "team_b"
    enabled = settings.team_b_enabled

    async def fetch(self) -> dict:
        # Mock: return synthetic data; replace with real Team B interface when available.
        return {
            "timestamp": datetime.now(timezone.utc).isoformat(),
            "readings": [
                {
                    "metric": metric,
                    "value": round(random.uniform(lo, hi), 2),
                    "unit": unit,
                }
                for metric, lo, hi, unit in _MOCK_METRICS
            ],
        }

    def normalize(self, raw: dict) -> list[NormalizedReading]:
        measured_at = datetime.fromisoformat(raw["timestamp"])
        return [
            NormalizedReading(
                source=self.source_name,
                location_id=_MOCK_LOCATION["location_id"],
                latitude=_MOCK_LOCATION["latitude"],
                longitude=_MOCK_LOCATION["longitude"],
                measured_at=measured_at,
                metric=r["metric"],
                value=r["value"],
                unit=r["unit"],
            )
            for r in raw["readings"]
        ]
