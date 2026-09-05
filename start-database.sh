#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")"
if docker compose version >/dev/null 2>&1; then
  docker compose up -d sql-server-db
else
  docker-compose up -d sql-server-db
fi
