from datetime import datetime, timezone

from influxdb_client.client.influxdb_client_async import InfluxDBClientAsync

from app.collectors.base import BaseCollector
from app.config import settings
from app.models.schemas import NormalizedReading

# Metropolia Myllypuro campus — where the MINNO weather station is deployed
_SENSOR_LOCATION = {
    "location_id": "team_b_minnowxsta",
    "latitude": 60.2230,
    "longitude": 25.0780,
}

# Map InfluxDB field name → (our metric name, unit)
# Fields confirmed by Team B. 'dr' (LoRaWAN data rate), 'payload_int', 'payload_raw' are excluded.
# PM2.5 field TBD — will be added when Team B confirms field name and unit.
_FIELD_MAP = {
    "temperature":    ("temperature",   "°C"),
    "humidity":       ("humidity",      "%"),
    "lux":            ("illuminance",   "lux"),
    "wind_direction": ("wind_direction","°"),
    "wind_speed":     ("wind_speed",    "m/s"),
    "rain_1h":        ("precipitation", "mm"),
}

_FLUX_QUERY = """
from(bucket: "{bucket}")
  |> range(start: -15m)
  |> filter(fn: (r) => r._measurement == "mqtt_consumer")
  |> filter(fn: (r) => contains(value: r._field, set: {fields}))
  |> last()
"""


class TeamBCollector(BaseCollector):
    source_name = "team_b"
    enabled = settings.team_b_enabled

    async def fetch(self) -> dict:
        fields_flux = "[" + ", ".join(f'"{f}"' for f in _FIELD_MAP) + "]"
        query = _FLUX_QUERY.format(
            bucket=settings.team_b_influx_bucket,
            fields=fields_flux,
        )
        readings = []
        async with InfluxDBClientAsync(
            url=settings.team_b_influx_url,
            token=settings.team_b_influx_token,
            org=settings.team_b_influx_org,
        ) as client:
            query_api = client.query_api()
            tables = await query_api.query(query)
            for table in tables:
                for record in table.records:
                    field = record.get_field()
                    if field not in _FIELD_MAP:
                        continue
                    readings.append({
                        "field": field,
                        "value": record.get_value(),
                        "time": record.get_time(),
                    })
        return {"readings": readings}

    def normalize(self, raw: dict) -> list[NormalizedReading]:
        results = []
        for r in raw["readings"]:
            metric, unit = _FIELD_MAP[r["field"]]
            ts = r["time"]
            if isinstance(ts, datetime):
                measured_at = ts.astimezone(timezone.utc).replace(tzinfo=timezone.utc)
            else:
                measured_at = datetime.now(timezone.utc)
            results.append(
                NormalizedReading(
                    source=self.source_name,
                    location_id=_SENSOR_LOCATION["location_id"],
                    latitude=_SENSOR_LOCATION["latitude"],
                    longitude=_SENSOR_LOCATION["longitude"],
                    measured_at=measured_at,
                    metric=metric,
                    value=float(r["value"]),
                    unit=unit,
                )
            )
        return results
