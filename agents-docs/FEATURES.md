# Feature Areas

This index represents the known feature areas in the system.

---

## Feature list (alphabetical)

- [Google Drive](features/google-drive.md) — OAuth and Drive file pointers for audio bytes; metadata stays in SQL Server.
- [Members](features/members.md) — member records and the member-information HTTP API.
- [Songs](features/songs.md) — band song lists and playback by song id.

Client bounded contexts live under `app/src/app/domains/<name>/README.md`. `agents-docs/features/<slug>.md` is for contracts that span client + API.

---

## Rules for agents

- Introducing a new feature area requires:
  - creating `agents-docs/features/<feature>.md` (use `agents-docs/features/feature-template.md`)
  - adding it to this list (alphabetical)
- This file should remain concise and navigable
