from sqlalchemy import BigInteger, Column, Double, Index, String, TIMESTAMP
from sqlalchemy.orm import DeclarativeBase
from sqlalchemy.sql import func


class Base(DeclarativeBase):
    pass


class SensorReading(Base):
    __tablename__ = "sensor_readings"

    id = Column(BigInteger, primary_key=True, autoincrement=True)
    source = Column(String(50), nullable=False)        # 'fmi', 'hsy', 'team_b'
    location_id = Column(String(100), nullable=False)  # station ID or area identifier
    latitude = Column(Double)                          # WGS84
    longitude = Column(Double)                         # WGS84
    measured_at = Column(TIMESTAMP(timezone=True), nullable=False)
    fetched_at = Column(TIMESTAMP(timezone=True), nullable=False, server_default=func.now())
    metric = Column(String(100), nullable=False)       # 'temperature', 'pm25', etc.
    value = Column(Double, nullable=False)
    unit = Column(String(20), nullable=False)          # '°C', 'µg/m³', '%', etc.

    __table_args__ = (
        Index("idx_readings_source_metric", "source", "metric", "measured_at"),
        Index("idx_readings_location", "location_id", "measured_at"),
    )
