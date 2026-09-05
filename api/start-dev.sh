#!/usr/bin/env bash
# Start the API in its own session. A later Cursor/agent SIGTERM on a
# foreground `dotnet run` must not take :5180 down.
set -euo pipefail
export PATH="${HOME}/.dotnet:${HOME}/.local/bin:${PATH}"
export DOTNET_ROOT="${DOTNET_ROOT:-${HOME}/.dotnet}"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

if curl -fsS -m 2 http://127.0.0.1:5180/api/health >/dev/null; then
  echo "TrackLink API already listening on :5180"
  exit 0
fi

mkdir -p api/logs
setsid -f env PATH="$PATH" DOTNET_ROOT="$DOTNET_ROOT" \
  dotnet run --project api --urls http://0.0.0.0:5180 \
  >>api/logs/dotnet-console.log 2>&1

for _ in $(seq 1 40); do
  if curl -fsS -m 1 http://127.0.0.1:5180/api/health >/dev/null; then
    echo "TrackLink API listening on :5180"
    exit 0
  fi
  sleep 0.5
done

echo "TrackLink API did not become ready on :5180" >&2
exit 1
