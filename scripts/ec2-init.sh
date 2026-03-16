#!/usr/bin/env bash
# One-time setup on EC2 before first deployment.
# Run as: bash scripts/ec2-init.sh
set -e

REPO_URL="https://github.com/<YOUR_ORG>/<YOUR_REPO>.git"
DEPLOY_PATH="/home/ubuntu/digital-twin-visual-env"

echo "==> Install Docker (skip if already installed)"
if ! command -v docker &>/dev/null; then
  curl -fsSL https://get.docker.com | sh
  sudo usermod -aG docker "$USER"
  echo "Docker installed. Re-login or run: newgrp docker"
fi

echo "==> Clone repo"
if [ ! -d "$DEPLOY_PATH/.git" ]; then
  git clone "$REPO_URL" "$DEPLOY_PATH"
fi

echo "==> Create backend/.env from .env.example"
if [ ! -f "$DEPLOY_PATH/backend/.env" ]; then
  cp "$DEPLOY_PATH/backend/.env.example" "$DEPLOY_PATH/backend/.env"
  echo "Edit $DEPLOY_PATH/backend/.env with real values before starting."
fi

echo "==> Done. Edit .env, then: cd $DEPLOY_PATH && docker compose up -d"
