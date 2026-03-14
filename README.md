# Visualizing Environmental Data in Digital Twin

## Purpose & Goals

The main goal of this project is to collect environmental data from open data sources and visualize it in a Digital Twin environment using Unity.

## Tasks in the Project

1. Innovate different ways to visualize environmental data in Digital Twins.
2. Investigate how environmental data should be stored effectively.
3. Test how external data can be read into Unity.
4. Create data visualizations in Unity.
5. Add data visualizations to the City of Helsinki's digital twin.

## Operating Mode

This is a fully agile project, and requirements may be updated during the project lifecycle.

## Members

- Jia Ke
- Ma Jing
- Pan Tingyu

---

## Architecture

```
Unity Frontend (Digital Twin)
        │
        │  REST API / HTTP polling (every 1–5 min)
        ▼
┌─────────────────────────────────────────┐
│           FastAPI Backend               │
│  /api/v1/environment/latest             │
│  /api/v1/environment/history            │
│  /api/v1/sources                        │
│  /api/v1/health                         │
└────────────┬──────────────┬─────────────┘
             │              │
             ▼              ▼
          Redis          PostgreSQL
        (TTL cache)    (persistent store)
             ▲              ▲
             └──────┬───────┘
                    │ APScheduler
        ┌───────────┼───────────┐
        ▼           ▼           ▼
   FMI Collector  HSY Collector  Team B (mock)
```

### Data Sources

| Source | Data | Update interval |
|--------|------|----------------|
| [FMI](https://en.ilmatieteenlaitos.fi/open-data) | Temperature, wind, humidity, pressure, precipitation | Every 10 min |
| [HSY](https://www.hsy.fi/en/air-quality-and-climate/air-quality/air-quality-in-the-hsy-region/) | Air Quality Index (AQI) | Every 60 min |
| Team B (mock) | Custom IoT sensor data | TBD |

### Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend API | FastAPI (Python) |
| Database | PostgreSQL |
| Cache | Redis |
| Scheduling | APScheduler |
| Containerization | Docker + docker-compose |
| Frontend | Unity |

---

## Backend Setup

### Prerequisites

- [Docker](https://docs.docker.com/get-docker/) and Docker Compose

### 1. Configure environment

```sh
cp backend/.env.example backend/.env
```

The default values work out of the box for local development. Edit `backend/.env` to change database credentials or disable/enable data sources.

### 2. Start all services

```sh
docker-compose up --build
```

This starts three containers:
- `api` — FastAPI application on port **8001**
- `postgres` — PostgreSQL database
- `redis` — Redis cache

### 3. Verify

| URL | Description |
|-----|-------------|
| http://localhost:8001/api/v1/health | Health check |
| http://localhost:8001/api/v1/sources | Data source status |
| http://localhost:8001/api/v1/environment/latest | Latest readings (all sources) |
| http://localhost:8001/docs | Interactive API documentation |

---

## API Reference

### `GET /api/v1/health`

Simple health check.

```json
{"status": "ok"}
```

### `GET /api/v1/environment/latest`

Returns the latest reading for each metric from all enabled sources.

**Query parameters** (both optional):

| Param | Description | Example |
|-------|-------------|---------|
| `source` | Filter by source | `fmi`, `hsy`, `team_b` |
| `metric` | Filter by metric | `temperature`, `aqi` |

**Example response:**

```json
{
  "fetched_at": "2026-03-14T10:00:00Z",
  "readings": [
    {
      "source": "fmi",
      "location_id": "fmisid_100971",
      "latitude": 60.17523,
      "longitude": 24.94459,
      "measured_at": "2026-03-14T09:50:00Z",
      "metric": "temperature",
      "value": 2.3,
      "unit": "°C"
    }
  ]
}
```

### `GET /api/v1/environment/history`

Returns time-series data for a specific metric and time range.

**Query parameters** (all required except `location_id`):

| Param | Description | Example |
|-------|-------------|---------|
| `source` | Data source | `fmi` |
| `metric` | Metric name | `temperature` |
| `from` | Start time (ISO 8601) | `2026-03-14T00:00:00Z` |
| `to` | End time (ISO 8601) | `2026-03-14T12:00:00Z` |
| `location_id` | Filter by station (optional) | `fmisid_100971` |

### `GET /api/v1/sources`

Returns the status of all configured data sources.

```json
{
  "sources": [
    {
      "name": "fmi",
      "enabled": true,
      "last_fetch_at": "2026-03-14T10:00:00Z",
      "last_error": null
    }
  ]
}
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
| `aqi` | index | HSY |

---

## Configuration

All settings are controlled via `backend/.env`:

```env
# Enable/disable data sources
FMI_ENABLED=true
HSY_ENABLED=true
TEAM_B_ENABLED=false

# Polling intervals
FMI_POLL_INTERVAL_MINUTES=10
HSY_POLL_INTERVAL_MINUTES=60

# Database & cache
POSTGRES_URL=postgresql+asyncpg://user:password@postgres:5432/envdata
REDIS_URL=redis://redis:6379/0
```

---

## Project Structure

```
.
├── backend/
│   ├── app/
│   │   ├── main.py              # FastAPI entry point
│   │   ├── config.py            # Settings from .env
│   │   ├── database.py          # PostgreSQL (SQLAlchemy async)
│   │   ├── cache.py             # Redis
│   │   ├── scheduler.py         # APScheduler jobs
│   │   ├── collectors/
│   │   │   ├── base.py          # Abstract collector interface
│   │   │   ├── fmi.py           # FMI WFS collector
│   │   │   ├── hsy.py           # HSY WFS collector
│   │   │   └── team_b.py        # Team B mock collector
│   │   ├── models/
│   │   │   ├── db.py            # SQLAlchemy ORM models
│   │   │   └── schemas.py       # Pydantic schemas
│   │   └── api/v1/
│   │       ├── health.py
│   │       ├── environment.py
│   │       └── sources.py
│   ├── Dockerfile
│   ├── requirements.txt
│   └── .env.example
├── docker-compose.yml
└── PROJECT_CONTEXT.md           # Architecture decisions and design notes
```
