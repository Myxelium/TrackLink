# Session Handoff

> **New chat:** attach `@agents-docs/HANDOFF.md`. Say: continue from this handoff; do not redo completed work.

**Status:** active

## Goal

Finish remaining musico board stories after the Google-session foundation. Leftover Drive catalog is hidden. Song versions are in. Next: search, then extra votes.

## Completed

- Drive OAuth, Google = TrackLink session, folder sandbox, invites
- Album catalogs + propose/approve + timestamped reviews
- Albums layout A
- Leftover Drive takes A (hide out-of-folder)
- Song versions A: same Drive file id returns the existing take; same name + new file is next version (`PreviousVersion` = prior song id); Takes shows `2 ← 1` and Drive modified day; MD5 stored when Drive sends it

## Changed files

- `api/Handlers/Bands/AddBandDriveSong.cs`, `ListBandSongs.cs`
- `api/Data/Migrations/20260905093200_SongVersionMeta.cs`
- `api.Tests/AddBandDriveSongTests.cs`
- `app/src/app/domains/song/take-version-label.ts`
- `app/src/app/features/studio/` Takes + propose picker
- Feature docs: `songs.md`

## Decisions

- Leftover Drive catalog A (hide, do not unlink)
- Versions A (not search or extra votes)
- `PreviousVersion` stays a prior song id, not a version number
- Re-linking the same Drive file id is idempotent

## Failed approaches

- Browser MCP often down
- Karma ChromeHeadless needs `--no-sandbox` on this host
- Jasmine `toContain('2 ← 1')` can lose the arrow; assert `\u2190` instead

## Current issue

Search and extra votes (name / art / order) are not built.

## Next steps

1. Manual: Sign in on `:4200`, link two Drive files with the same name — Takes should show `2 ← 1` and the Drive day
2. Interview then build search or extra votes

## Commands

```bash
export PATH="$HOME/.dotnet:$HOME/.local/bin:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"
curl -s http://localhost:5180/api/bands/1/songs
# Sign in: http://localhost:4200/
```

## Local JunoLint (do not redo)

`app/` uses `file:../../JunoLint` (`/mnt/Kindred_ext4/repos/JunoLint`). Dev Container must bind-mount that sibling. Do not disable the sentence-name rules; rename when touching those files.
