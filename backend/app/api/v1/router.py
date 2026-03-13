from fastapi import APIRouter

from app.api.v1 import environment, health, sources

router = APIRouter(prefix="/api/v1")

router.include_router(health.router)
router.include_router(environment.router)
router.include_router(sources.router)
