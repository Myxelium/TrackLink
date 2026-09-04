# Session Handoff

> **New chat:** attach `@agents-docs/HANDOFF.md`. Say: continue from this handoff; do not redo completed work.

**Status:** active

## Goal

Prove Google Drive playback end-to-end, and make local unpublished JunoLint resolve inside the Dev Container (and CI if you touch workflows).

## Completed

- Musico board UI/API stories except Google Drive + OAuth (those are coded, not proven)
- Studio first page: member picker, member info, band songs, HTML5 player via `/api/songs/{id}/audio`
- APIs: `GET /api/members`, `GET /api/members/{id}`, `GET /api/bands/{id}/songs`, `GET /api/songs/{id}/audio` (Range 206)
- Agent/rules layout (metoyou-shaped): `AGENTS.md`, `agents-docs/`, `.cursor/rules/`
- Dev Container at repo root: Node 22, Angular CLI, .NET 9 SDK, SQL Server 2022 (`sql-server-db`), ports 4200/4000/9876/5180/1433
- Unpublished JunoLint `file:../../JunoLint` (`/mnt/Kindred_ext4/repos/JunoLint`). Extra README-off rules on `**/*.ts`: `prefer-sentence-names`, `prefer-sentence-function-names` (`warn`, `minLength: 0`), `decompose-complex-expressions` (`warn`, `threshold: 5`). `cd app && npm run lint` is clean. Dropped leading-`the` names so `no-leading-the` passes.

## Changed files

- `api/` members, songs audio, Google OAuth/Drive scaffolding
- `app/` studio UI, `eslint.config.js`, `package.json` junolint path
- `.devcontainer/` Node + .NET 9 + compose SQL
- `AGENTS.md`, `agents-docs/`, `.cursor/rules/`

## Decisions

- `Song.Url` is `https://...` or `gdrive:{fileId}` (ADR-0002)
- One Google account per API process for v1 (not per member)
- API is `net9.0`
- JunoLint stays unpublished/local until you publish; names must be multi-word **and** must not start with the word `the`

## Failed approaches

- Draft issue bodies on the GitHub project were not readable without login; titles + status only
- IDE browser MCP was down; studio page checked via `ng serve` + `/api` proxy, not click-through
- Host had no Chrome for Karma; Chromium is in the Dev Container
- Dev Container does not see sibling `/mnt/Kindred_ext4/repos/JunoLint`, so `file:../../JunoLint` breaks inside the container until bind-mounted

## Current issue

1. `Google__ClientId` / `Google__ClientSecret` empty → `GET /api/auth/google/status` is `configured: false`; login is 503
2. Dev Container (and GitHub `npm ci`) cannot resolve the sibling JunoLint path

## Next steps

1. Bind-mount JunoLint in `.devcontainer/devcontainer.json` so `app/`'s `../../JunoLint` is `/workspaces/JunoLint` (or pack/publish junolint for CI)
2. Create a Google Cloud OAuth web client, Drive API on, redirect `http://localhost:5180/api/auth/google/callback`
3. Set `Google__ClientId` and `Google__ClientSecret` (user secrets ok)
4. Open `/api/auth/google/login`, confirm status `connected: true`
5. Set a song `Url` to `gdrive:{fileId}` and play `/api/songs/{id}/audio`

## Commands

```bash
# host
./start-database.sh
dotnet run --project api --urls http://localhost:5180
cd app && npm start

# Dev Container (after rebuild)
dotnet run --project api --urls http://0.0.0.0:5180
npm --prefix app start -- --host 0.0.0.0

cd app && npm run lint
```
