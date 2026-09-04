# Engineering Standards & Workflows

This document defines shared engineering practices for **TrackLink**.

---

## Root README.md policy

`README.md` exists to answer:

- what this repo is
- how to run it locally
- where to find canonical documentation

Agents should update `README.md` when dev commands change, ports or startup steps change, or links to docs move.

Agents should **not** describe feature behavior, list API endpoints, or include request/response schemas. Canonical documentation lives under `agents-docs/` and (for client bounded contexts) under `app/src/app/domains/<name>/README.md`.

---

## Testing standards

### Unit / component tests — Karma + Jasmine

- **Where it runs:** Angular client (`app/`)
- **Test suffix:** `*.spec.ts`
- **Location:** colocated with source
- **Run:** `cd app && npm test`
- **Single file:** `cd app && npx ng test --include=src/app/path/to/file.spec.ts`

### API

- Prefer a focused HTTP check (`curl` against `http://localhost:5180`) or a handler test when adding non-trivial query logic.
- Do not require SQL Server for pure mapping tests.

### Test-backed development (balanced)

See `/AGENTS.md` § CRITICAL. Done = the asked behavior works, proven at the right level.

---

## TypeScript / C# standards

- Angular `strict` templates and TypeScript strict mode are on
- Avoid `any` / `dynamic` unless necessary; document why if used
- Junolint (ESLint flat config) owns Angular TS + HTML layout. Run `npm run lint:fix` from `app/` — do not hand-fix auto-fixable issues
- `dotnet build` type-checks the API (`net9.0`)

---

## Naming conventions

- Angular: kebab-case files (`song-list.component.ts`), `*.component.html`, `*.component.scss`
- Domain rules (pure functions): `*.rules.ts`
- C# handlers live under `api/Handlers/<Area>/` as nested Command + Handler types (MediatR)
- HTTP routes are `api/<resource>`

---

## Local run

1. `./start-database.sh` (Docker SQL Server on `:1433`)
2. `dotnet run --project api` (http://localhost:5180)
3. `cd app && npm start` (http://localhost:4200, proxies `/api` to the API)

Google Drive OAuth needs `Google__ClientId` and `Google__ClientSecret`. Without them the API still serves URL-backed songs.
