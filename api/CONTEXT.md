# API

Owns TrackLink metadata, member queries, audio playback by song id, and Google Drive OAuth.

## Vocabulary

| Term | Definition | Aliases to avoid |
|------|------------|------------------|
| **Member** | Person row (`Username`, `UserIdentifier`). | "user" |
| **Band** | Collaboration group. | "artist" when you mean the group |
| **Song** | Metadata + `Url` pointer. | "blob", "file" |
| **Song identifier** | Join of a song to a band. | "playlist item" |
| **Drive pointer** | `gdrive:{fileId}` stored on `Song.Url`. | "Drive URL" when it is only an id |

## Relationships

- **Band** *—* **Member** via **BandMember**.
- **Band** *—* **Song** via **SongIdentifier**.
- **Member** uploads **Song** (`UploadedBy`).
- **Google account** tokens are API-side, not a Member login replacement.

## Boundaries / IO

- **Exposes:** `/api/members`, `/api/members/{id}`, `/api/bands/{id}/songs`, `/api/songs/{id}/audio`, `/api/auth/google/*`, `/api/drive/files`.
- **Consumes:** SQL Server; optional Google OAuth + Drive API.

## Invariants

- `Song.Url` is a pointer, never embedded bytes.
- Playback supports HTTP Range.
- Missing Google credentials must not take down member/song endpoints.

## Flagged ambiguities

- One Google account per API instance for v1 (not per member). Per-member Drive tokens need a product decision.
