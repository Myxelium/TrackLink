#!/usr/bin/env bash
set -euo pipefail

if [[ -f app/package.json ]]; then
  echo "Installing npm dependencies in app/..."
  npm --prefix app install
else
  echo "app/package.json not found; skipping npm install."
fi

echo
echo "Dev Container ready (workspace is the repo root)."
echo "  Angular:  npm --prefix app start -- --host 0.0.0.0"
echo "  SSR:      npm --prefix app run serve:ssr:app"
echo "  Tests:    npm --prefix app test -- --watch=false"
