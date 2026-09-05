# API guidelines

ASP.NET Core 9 API: MediatR handlers, EF Core + SQL Server, Swagger in Development.

## Workflow

- Run detached so a Cursor/agent shell abort does not SIGTERM the host: `bash api/start-dev.sh` (http://localhost:5180). Do **not** leave `dotnet run` in a foreground agent shell.
- Logs: `api/logs/tracklink-.log` (rolling daily; tail with `tail -f api/logs/tracklink-*.log`)
- Build: `dotnet build api/api.csproj`
- Migrations apply on startup when `Migrate` is true (`appsettings.Development.json`)

## Boundaries

- Controllers stay thin: send MediatR commands/queries, or call `IAudioPlaybackService` for streams.
- Google SDK types stay in `Integrations/Google` — handlers talk to `IGoogleDriveService`.
- Do not store audio bytes in EF entities.

## Before you finish

- If HTTP contracts changed, update `agents-docs/features/<area>.md`.
- `dotnet build` the API.
