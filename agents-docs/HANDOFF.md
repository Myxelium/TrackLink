# Session Handoff

> **New chat:** attach `@agents-docs/HANDOFF.md`. Say: continue from this handoff; do not redo completed work.

**Status:** active

## Goal

Finish remaining musico board stories after the Google-session foundation. Album desk, timestamped reviews, and the master-detail Albums layout are in. Next: leftover Drive catalog, then votes, versions, search.

## Completed

- Drive OAuth, Google = TrackLink session, folder sandbox, invites
- Album catalogs + propose/approve (`all` / `owner_uploaders`)
- Timestamped reviews (`startMs`/`endMs` + comment; click seeks transport)
- Reviews allowed after approval so a solo owner can still comment
- Solo-owner **Put on album** FK fix: admit via `Proposal` navigation, not `ProposalId = 0`
- Albums layout **A**: slim list left, album/PR workspace right

## Changed files

- `api/` albums, proposals, reviews, migration `20260904194500_ProposalReviews`
- `api.Tests/AlbumDeskTests.cs`
- `app/src/app/features/studio/` album desk/work, deck seek, page layout
- Feature docs: `albums.md`, `proposals.md`

## Decisions

- Any band member can comment; author or owner can delete
- Mark start/end from the transport playhead (no waveform)
- Approved proposals stay commentable; withdrawn does not
- Layout A (not single-column or three-pane)
- Stale Drive catalog A/B/C **not chosen**

## Failed approaches

- `@else if (x(); as y)` is invalid Angular — nest `@if` in `@else`
- `new Component()` with `input()` throws NG0203
- `ProposalId = proposal.Id` before insert fails `FK_AlbumTrack_Proposal` (InMemory hides it)
- Browser MCP often down in this environment

## Current issue

Takes still lists Drive files from the old full-Drive scan. Propose of those can be `outside_folder`; the studio catch-all says “must already be on this band.” Votes, versions (date/MD5), and search are not built.

## Next steps

1. Manual: Sign in on `:4200`, Albums rail — confirm slim list + workspace, Put on album, review seek
2. Leftover Drive takes: **A** hide out-of-folder, **B** unlink, **C** fix error text only
3. Votes, versions, search

## Commands

```bash
export PATH="$HOME/.dotnet:$HOME/.local/bin:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"
export NG_ALLOWED_HOSTS="localhost,127.0.0.1"

curl -s http://localhost:5180/api/health
# Sign in: http://localhost:4200/
# Albums is the third studio rail button
```

## Local JunoLint (do not redo)

`app/` uses `file:../../JunoLint` (`/mnt/Kindred_ext4/repos/JunoLint`). Dev Container must bind-mount that sibling. Do not disable the sentence-name rules; rename when touching those files.
