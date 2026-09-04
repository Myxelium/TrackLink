#!/usr/bin/env bash
set -euo pipefail

if [[ -f app/package.json ]]; then
  echo "Installing npm dependencies in app/..."
  npm --prefix app install
else
  echo "app/package.json not found; skipping npm install."
fi

if [[ -f api/api.csproj ]]; then
  echo "Restoring .NET API packages..."
  dotnet restore api/api.csproj
else
  echo "api/api.csproj not found; skipping dotnet restore."
fi

echo
echo "Dev Container ready (workspace is the repo root)."
echo "  Angular:  npm --prefix app start -- --host 0.0.0.0"
echo "  SSR:      npm --prefix app run serve:ssr:app"
echo "  Tests:    npm --prefix app test -- --watch=false"
echo "  API:      dotnet run --project api --urls http://0.0.0.0:5180"
echo "  Database: SQL Server on sql-server-db:1433 (published to host :1433)."
echo "            Connection string is set via ConnectionStrings__Database."
