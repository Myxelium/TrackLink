# Feature Areas

This index represents the known feature areas in the system.

---

## Feature list (alphabetical)

- [Albums](features/albums.md) — band catalogs of admitted takes; no audio bytes in SQL Server.
- [Google Drive](features/google-drive.md) — per-member Google login, band folder root, and Drive file pointers; metadata stays in SQL Server.
- [Invites](features/invites.md) — owner email invites and `/join` accept-by-code.
- [Members](features/members.md) — member records created from Google login.
- [Proposals](features/proposals.md) — propose a take onto an album; approve or reject like a small PR.
- [Songs](features/songs.md) — band song lists and playback by song id.

Client bounded contexts live under `app/src/app/domains/<name>/README.md`. `agents-docs/features/<slug>.md` is for contracts that span client + API.

---

## Rules for agents

- Introducing a new feature area requires:
  - creating `agents-docs/features/<feature>.md` (use `agents-docs/features/feature-template.md`)
  - adding it to this list (alphabetical)
- This file should remain concise and navigable
