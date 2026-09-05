#!/usr/bin/env bash
set -euo pipefail

host="${SQL_HOST:-sql-server-db}"
port="${SQL_PORT:-1433}"

echo "Waiting for SQL Server at ${host}:${port}..."
for i in $(seq 1 60); do
  if timeout 1 bash -c "echo > /dev/tcp/${host}/${port}" 2>/dev/null; then
    # sqlservr can bind 1433 before it accepts logins.
    sleep 5
    echo "SQL Server is reachable at ${host}:${port}."
    exit 0
  fi
  sleep 2
done

echo "WARNING: SQL Server at ${host}:${port} did not become reachable. Try: docker compose logs sql-server-db" >&2
exit 0
