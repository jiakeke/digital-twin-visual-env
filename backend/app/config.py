from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    # Data sources
    fmi_enabled: bool = True
    hsy_enabled: bool = True
    team_b_enabled: bool = False

    # FMI
    fmi_wfs_url: str = "https://opendata.fmi.fi/wfs"
    fmi_location: str = "helsinki"
    fmi_poll_interval_minutes: int = 10

    # HSY
    hsy_wfs_url: str = "https://kartta.hsy.fi/geoserver/ilmanlaatu/wfs"
    hsy_poll_interval_minutes: int = 60

    # Team B (TBD)
    team_b_db_url: str = ""

    # Database
    postgres_url: str = "postgresql+asyncpg://user:password@postgres:5432/envdata"

    # Redis
    redis_url: str = "redis://redis:6379/0"

    model_config = {"env_file": ".env", "env_file_encoding": "utf-8"}


settings = Settings()
