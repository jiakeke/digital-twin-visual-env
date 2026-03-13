import json

import redis.asyncio as aioredis

from app.config import settings

_redis: aioredis.Redis | None = None


def get_redis() -> aioredis.Redis:
    global _redis
    if _redis is None:
        _redis = aioredis.from_url(settings.redis_url, decode_responses=True)
    return _redis


async def set_latest(source: str, metric: str, location_id: str, data: dict, ttl: int) -> None:
    key = f"latest:{source}:{metric}:{location_id}"
    await get_redis().set(key, json.dumps(data), ex=ttl)


async def get_latest(source: str | None = None, metric: str | None = None) -> list[dict]:
    r = get_redis()
    pattern = f"latest:{source or '*'}:{metric or '*'}:*"
    keys = await r.keys(pattern)
    if not keys:
        return []
    values = await r.mget(*keys)
    return [json.loads(v) for v in values if v is not None]


async def close_redis() -> None:
    global _redis
    if _redis:
        await _redis.aclose()
        _redis = None
