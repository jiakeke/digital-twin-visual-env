# PROJECT_CONTEXT.md
# Digital Twin Environmental Data Visualization — Helsinki

> This document captures all architectural decisions and context from the initial design session.
> Provide this file to Claude Code at the start of each session for full context.

---

## Project Overview

**Goal**: Visualize environmental data in the City of Helsinki's Digital Twin using Unity.

**Two main components**:
- **Unity frontend** — 3D Digital Twin visualization of environmental data
- **FastAPI backend** — Data aggregation service that collects, stores, and serves environmental data to Unity

**Operating mode**: Fully agile — requirements may evolve during the project.

---

## Tech Stack Decisions

| Layer | Technology | Reason |
|-------|-----------|--------|
| Backend API | **FastAPI (Python)** | Async-native (ideal for concurrent API calls), lightweight, auto OpenAPI docs |
| Database | **PostgreSQL** | Persistent historical environmental readings |
| Cache | **Redis** | Latest readings per source/metric, TTL-based, fast Unity polling |
| Scheduling | **APScheduler** | Periodic data collection from each source |
| Containerization | **Docker + docker-compose** | Deployment-environment agnostic (local → school server → city cloud) |
| Frontend | **Unity** | 3D Digital Twin visualization |
| Communication | **REST API (HTTP polling)** | Unity polls backend every 1–5 minutes; no WebSocket needed at this stage |

**Why FastAPI over Django REST Framework**: This backend is a data aggregation and forwarding service — it does not need Django's ORM, admin panel, or template engine. FastAPI's async support is critical for concurrent calls to multiple external APIs.

**Why REST over WebSocket**: Environmental data updates at minute/hourly intervals. REST polling every 1–5 minutes is sufficient. WebSocket can be added later if Team B's sensor data requires second-level streaming.

---

## Data Sources

### 1. FMI — Finnish Meteorological Institute
- **Data types**: Temperature, humidity, air pressure, wind speed/direction, precipitation, air quality, weather forecasts
- **Protocol**: OGC WFS (Web Feature Service) over HTTP
- **Endpoint**: `https://opendata.fmi.fi/wfs`
- **Data format**: XML/GML (`multipointcoverage` or `timevaluepair` format)
- **Update frequency**: Weather observations every **10 minutes**, forecasts updated hourly
- **Authentication**: Free, no API key required
- **Parsing**: Use `xmltodict` or `lxml` + `owslib` for WFS handling
- **Recommended polling interval**: Every **10 minutes**

**Example stored query**:
```
https://opendata.fmi.fi/wfs?service=WFS&version=2.0.0&request=getFeature
  &storedquery_id=fmi::observations::weather::multipointcoverage
  &place=helsinki
```

### 2. HSY — Helsinki Region Environmental Services
- **Data types**: PM2.5, PM10, NO₂, O₃, SO₂, Air Quality Index (AQI), land use data
- **Protocol**: OGC WFS + WMS + WMTS over HTTP
- **Endpoints**:
  - Air quality (real-time): `https://kartta.hsy.fi/geoserver/ilmanlaatu/wfs`
  - General geodata: `https://kartta.hsy.fi/geoserver/wfs`
- **Data format**: GML, GeoJSON, Shapefile
- **Update frequency**: Real-time air quality updated **every hour**; geographic/land use data is static (updated annually)
- **Authentication**: Free, no API key required
- **Coordinate system**: ETRS-GK25 (**EPSG:3879**) — must convert to WGS84 (EPSG:4326) using `pyproj`
- **Recommended polling interval**: Every **60 minutes**

### 3. Team B — Sensor Data (TBD)
- **Data types**: Unknown — custom IoT sensor readings from field deployment
- **Protocol**: Direct database access (assumed) — exact interface TBD
- **Data format**: TBD
- **Update frequency**: TBD (likely sensor-level, potentially sub-minute)
- **Status**: Interface not yet designed. Use **mock data** during development.
- **Design principle**: Implement as a pluggable collector — swap mock for real implementation when Team B's interface is confirmed.

### Excluded Sources
- **HRI (Helsinki Region Infoshare)**: Primarily statistical and geographic background data, not real-time sensor data. Excluded from active data collection. May be revisited for static background layers.

---

## Architecture

```
Unity Frontend (Digital Twin)
        │
        │  REST API / HTTP polling (every 1–5 min)
        ▼
┌─────────────────────────────────────────┐
│           FastAPI Backend               │
│  /api/v1/environment  (latest data)     │
│  /api/v1/history      (time range)      │
│  /api/v1/sources      (source status)   │
└────────────┬──────────────┬─────────────┘
             │ read latest  │ read history
             ▼              ▼
          Redis          PostgreSQL
        (TTL cache)    (persistent store)
             ▲              ▲
             └──────┬───────┘
                    │ write (APScheduler)
        ┌───────────┼───────────┐
        ▼           ▼           ▼
   FMI Collector  HSY Collector  TeamB Collector
        │           │               │
        ▼           ▼               ▼
    FMI WFS      HSY WFS         Mock / DB
    (XML/GML)   (GML/GeoJSON)    (TBD)
```

---

## Backend Internal Module Structure

```
backend/
├── app/
│   ├── main.py                  # FastAPI app entry point
│   ├── config.py                # Settings from .env (which sources enabled, DB URLs, etc.)
│   ├── database.py              # PostgreSQL connection (SQLAlchemy async)
│   ├── cache.py                 # Redis connection (aioredis)
│   │
│   ├── collectors/              # One collector per data source
│   │   ├── base.py              # Abstract base class: fetch() + normalize()
│   │   ├── fmi.py               # FMI WFS collector
│   │   ├── hsy.py               # HSY WFS collector
│   │   └── team_b.py            # Team B collector (mock for now)
│   │
│   ├── scheduler.py             # APScheduler jobs — triggers each collector
│   │
│   ├── models/
│   │   ├── db.py                # SQLAlchemy ORM models
│   │   └── schemas.py           # Pydantic schemas (API request/response)
│   │
│   └── api/
│       └── v1/
│           ├── router.py        # Combines all route modules
│           ├── environment.py   # /environment endpoints
│           └── health.py        # /health endpoint
│
├── tests/
├── .env.example
├── requirements.txt
├── Dockerfile
└── docker-compose.yml
```

---

## Database Schema

Single unified table for all sources (avoids multi-table JOINs in Unity queries):

```sql
CREATE TABLE sensor_readings (
    id            BIGSERIAL PRIMARY KEY,
    source        VARCHAR(50)   NOT NULL,   -- 'fmi', 'hsy', 'team_b'
    location_id   VARCHAR(100)  NOT NULL,   -- station ID or area identifier
    latitude      DOUBLE PRECISION,         -- WGS84
    longitude     DOUBLE PRECISION,         -- WGS84
    measured_at   TIMESTAMPTZ   NOT NULL,   -- original measurement timestamp
    fetched_at    TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    metric        VARCHAR(100)  NOT NULL,   -- 'temperature', 'pm25', 'no2', etc.
    value         DOUBLE PRECISION NOT NULL,
    unit          VARCHAR(20)   NOT NULL    -- '°C', 'µg/m³', '%', etc.
);

CREATE INDEX idx_readings_source_metric ON sensor_readings(source, metric, measured_at DESC);
CREATE INDEX idx_readings_location ON sensor_readings(location_id, measured_at DESC);
```

### Redis Key Pattern
```
latest:{source}:{metric}:{location_id}  →  JSON value + timestamp
```
TTL per source:
- FMI: 15 minutes (10 min update interval × 1.5)
- HSY: 90 minutes (60 min update interval × 1.5)
- Team B: TBD

---

## Collector Interface Contract

Every collector must implement this interface:

```python
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
```

### NormalizedReading Schema
```python
class NormalizedReading(BaseModel):
    source: str           # 'fmi', 'hsy', 'team_b'
    location_id: str      # station or area ID
    latitude: float       # WGS84
    longitude: float      # WGS84
    measured_at: datetime
    metric: str           # standardized metric name (see below)
    value: float
    unit: str
```

### Standardized Metric Names
| Metric | Unit | Source |
|--------|------|--------|
| `temperature` | °C | FMI |
| `humidity` | % | FMI |
| `wind_speed` | m/s | FMI |
| `wind_direction` | ° | FMI |
| `pressure` | hPa | FMI |
| `precipitation` | mm/h | FMI |
| `pm25` | µg/m³ | HSY |
| `pm10` | µg/m³ | HSY |
| `no2` | µg/m³ | HSY |
| `o3` | µg/m³ | HSY |
| `so2` | µg/m³ | HSY |
| `aqi` | index | HSY |

---

## API Endpoints (Unity-facing)

### GET `/api/v1/environment/latest`
Returns latest reading for each metric from all enabled sources.

**Query params**: `source` (optional filter), `metric` (optional filter)

**Response**:
```json
{
  "fetched_at": "2026-03-14T10:00:00Z",
  "readings": [
    {
      "source": "fmi",
      "location_id": "helsinki_mannerheimintie",
      "latitude": 60.1699,
      "longitude": 24.9384,
      "measured_at": "2026-03-14T09:50:00Z",
      "metric": "temperature",
      "value": 2.3,
      "unit": "°C"
    }
  ]
}
```

### GET `/api/v1/environment/history`
Returns time-series data for a metric in a time range.

**Query params**: `source`, `metric`, `location_id`, `from`, `to`

### GET `/api/v1/sources`
Returns status of all configured data sources (enabled, last fetch time, last error).

### GET `/api/v1/health`
Simple health check for Docker/deployment monitoring.

---

## Configuration (.env)

```env
# Data sources (enable/disable per source)
FMI_ENABLED=true
HSY_ENABLED=true
TEAM_B_ENABLED=false

# FMI
FMI_WFS_URL=https://opendata.fmi.fi/wfs
FMI_LOCATION=helsinki
FMI_POLL_INTERVAL_MINUTES=10

# HSY
HSY_WFS_URL=https://kartta.hsy.fi/geoserver/ilmanlaatu/wfs
HSY_POLL_INTERVAL_MINUTES=60

# Team B (TBD)
TEAM_B_DB_URL=

# Database
POSTGRES_URL=postgresql+asyncpg://user:password@postgres:5432/envdata

# Redis
REDIS_URL=redis://redis:6379/0
```

---

## Docker Setup

Three containers via `docker-compose`:
1. `api` — FastAPI application
2. `postgres` — PostgreSQL database
3. `redis` — Redis cache

Designed to be deployment-environment agnostic: runs locally for development, can be deployed to any server or cloud with Docker support.

---

## Key Implementation Notes

1. **Coordinate conversion**: HSY data uses EPSG:3879 (Finnish local CRS). Always convert to WGS84 using `pyproj` in the HSY collector's `normalize()` method before storing.

2. **WFS XML parsing**: FMI and HSY both use OGC WFS which returns XML/GML. Use `httpx` for async HTTP + `xmltodict` or `owslib` for parsing. Do not expect JSON responses from these endpoints.

3. **Team B mock**: Until Team B's interface is defined, `team_b.py` should return realistic mock data (random values within plausible ranges for Helsinki) so Unity development is not blocked.

4. **Graceful degradation**: If one collector fails (network error, source downtime), the others continue running. Errors are logged and visible via `/api/v1/sources`.

5. **Unity polling**: Unity calls `/api/v1/environment/latest` every 1–5 minutes. This endpoint reads from Redis (fast), falling back to PostgreSQL if Redis is empty.

---

## Current Status

- [x] Architecture designed
- [x] Data sources researched (FMI, HSY confirmed; HRI excluded; Team B TBD)
- [x] Project scaffolding / directory structure created
- [x] Docker compose setup
- [x] Base collector implementation
- [x] FMI collector (fetch + normalize — stdlib ElementTree, no extra deps)
- [x] HSY collector (fetch + normalize — GeoJSON + pyproj EPSG:3879→WGS84)
- [x] Team B mock collector
- [x] FastAPI routes
- [ ] Unity client implementation

---

*Last updated: 2026-03-14 — FMI and HSY collectors implemented*
