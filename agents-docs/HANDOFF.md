# Session Handoff

> **New chat:** attach `@agents-docs/HANDOFF.md`. Say: continue from this handoff; do not redo completed work.

**Status:** active

## Goal

Finish the two In Progress musico board items that need a Google Cloud OAuth client: Drive integration and Google OAuth.

## Completed

- Todo UI/API stories from https://github.com/users/Myxelium/projects/1 (titles only; draft bodies are not public without GitHub login)
- Agent/rules structure (metoyou-shaped)
- Junolint in `app/` (`npm run lint` clean)
- Member information, band songs, play-by-id audio (Range 206 verified)
- Google OAuth/Drive code paths; they 503 until credentials exist

## Changed files

- `api/` members, bands/{id}/songs, songs/{id}/audio, auth/google/*, Drive service
- `app/` studio first page + junolint
- `AGENTS.md`, `agents-docs/`, `.cursor/rules/`

## Decisions

- `Song.Url` is a pointer: `https://...` or `gdrive:{fileId}` (ADR-0002)
- One Google account per API process for v1 (not per member)
- API retargeted to `net9.0` so it runs on this machine's installed runtime

## Failed approaches

- `gh` is not installed; project items were read from the public project HTML (titles + status only)
- Cursor browser MCP was unavailable; first page verified via `ng serve` HTML/JS + `/api` proxy

## Current issue

Google stories cannot be proven end-to-end: `Google__ClientId` and `Google__ClientSecret` are empty. `GET /api/auth/google/status` returns `configured: false`. `GET /api/auth/google/login` returns 503.

## Next steps

1. Create a Google Cloud OAuth client (web) with Drive API enabled
2. Redirect URI: `http://localhost:5180/api/auth/google/callback`
3. Set `Google__ClientId` and `Google__ClientSecret` (or `Google:ClientId` / `Google:ClientSecret` in user secrets)
4. Open `/api/auth/google/login`, consent, confirm status `connected: true`
5. Point a song `Url` at `gdrive:{fileId}` and play `/api/songs/{id}/audio`

## Commands

```bash
./start-database.sh
dotnet run --project api --urls http://localhost:5180
cd app && npm start
# then: GET http://localhost:5180/api/auth/google/status
```
