from abc import ABC, abstractmethod
from typing import List

from app.models.schemas import NormalizedReading


class BaseCollector(ABC):
    source_name: str  # 'fmi' | 'hsy' | 'team_b'
    enabled: bool     # from config/.env

    @abstractmethod
    async def fetch(self) -> dict:
        """Fetch raw data from the source API or database."""
        ...

    @abstractmethod
    def normalize(self, raw: dict) -> List[NormalizedReading]:
        """Transform raw source data into unified NormalizedReading objects.
        Must convert coordinates to WGS84 (EPSG:4326) if needed."""
        ...

    async def run(self) -> List[NormalizedReading]:
        """Fetch + normalize. Called by scheduler."""
        raw = await self.fetch()
        return self.normalize(raw)
