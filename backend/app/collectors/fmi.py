"""FMI (Finnish Meteorological Institute) WFS collector.

Fetches weather observations from the FMI open data WFS endpoint using the
multipointcoverage stored query.  Each call returns the last ~12 hours of
10-minute observations for Helsinki.  We extract only the most recent row.

Observed parameters (order matters — matches the tuple list in the response):
  0  t2m        temperature      °C
  1  ws_10min   wind_speed       m/s
  2  wg_10min   wind_gust        m/s   (not stored)
  3  wd_10min   wind_direction   °
  4  rh         humidity         %
  5  td         dew_point        °C    (not stored)
  6  r_1h       precipitation    mm/h
  7  ri_10min   rain_intensity   mm/h  (not stored)
  8  snow_aws   snow_depth       cm    (not stored)
  9  p_sea      pressure         hPa
  10 vis        visibility       m     (not stored)
  11 n_man      cloud_cover      okta  (not stored)
  12 wawa       weather_symbol   code  (not stored)
"""

from datetime import datetime, timezone
from xml.etree import ElementTree as ET

import httpx

from app.collectors.base import BaseCollector
from app.config import settings
from app.models.schemas import NormalizedReading

_STORED_QUERY = "fmi::observations::weather::multipointcoverage"

_PARAM_NAME_MAP: dict[str, tuple[str, str]] = {
    "t2m":      ("temperature", "°C"),
    "ws_10min": ("wind_speed", "m/s"),
    "wd_10min": ("wind_direction", "°"),
    "rh":       ("humidity", "%"),
    "r_1h":     ("precipitation", "mm/h"),
    "p_sea":    ("pressure", "hPa"),
}

_NS = {
    "wfs":    "http://www.opengis.net/wfs/2.0",
    "omso":   "http://inspire.ec.europa.eu/schemas/omso/3.0",
    "om":     "http://www.opengis.net/om/2.0",
    "gml":    "http://www.opengis.net/gml/3.2",
    "gmlcov": "http://www.opengis.net/gmlcov/1.0",
    "xlink":  "http://www.w3.org/1999/xlink",
}


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
        root = ET.fromstring(raw["xml"])
        readings: list[NormalizedReading] = []

        for member in root.findall("wfs:member", _NS):
            obs = member.find("omso:GridSeriesObservation", _NS)
            if obs is None:
                continue

            # Station ID
            fmisid_el = obs.find(".//gml:identifier", _NS)
            if fmisid_el is None or not fmisid_el.text:
                continue
            location_id = f"fmisid_{fmisid_el.text.strip()}"

            # Coordinates (EPSG:4258 ≈ WGS84)
            point_el = obs.find(".//gml:Point/gml:pos", _NS)
            if point_el is None or not point_el.text:
                continue
            parts = point_el.text.strip().split()
            lat, lon = float(parts[0]), float(parts[1])

            # Build index→(metric, unit) from the observedProperty href
            prop_el = obs.find("om:observedProperty", _NS)
            href = prop_el.get("{http://www.w3.org/1999/xlink}href", "") if prop_el is not None else ""
            params_str = href.split("param=")[1].split("&")[0] if "param=" in href else ""
            param_idx_map: dict[int, tuple[str, str]] = {
                idx: _PARAM_NAME_MAP[p]
                for idx, p in enumerate(params_str.split(","))
                if p in _PARAM_NAME_MAP
            }

            # Positions and values
            pos_el = obs.find(".//gmlcov:positions", _NS)
            val_el = obs.find(".//gml:doubleOrNilReasonTupleList", _NS)
            if pos_el is None or val_el is None:
                continue

            pos_rows = [r.strip() for r in pos_el.text.strip().splitlines() if r.strip()]
            val_rows = [r.strip() for r in val_el.text.strip().splitlines() if r.strip()]
            if not pos_rows or not val_rows:
                continue

            # Latest row only
            last_pos_parts = pos_rows[-1].split()
            unix_ts = float(last_pos_parts[2])
            measured_at = datetime.fromtimestamp(unix_ts, tz=timezone.utc)
            last_vals = val_rows[-1].split()

            for idx, (metric, unit) in param_idx_map.items():
                if idx >= len(last_vals):
                    continue
                raw_val = last_vals[idx]
                if raw_val in ("NaN", "nan", ""):
                    continue
                readings.append(
                    NormalizedReading(
                        source=self.source_name,
                        location_id=location_id,
                        latitude=lat,
                        longitude=lon,
                        measured_at=measured_at,
                        metric=metric,
                        value=float(raw_val),
                        unit=unit,
                    )
                )

        return readings
