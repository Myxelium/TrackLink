# API

Owns TrackLink metadata, Google login → Member upsert, per-member Drive tokens, band folder sandbox, album catalogs, proposals, invites, and audio playback by song id.

## Vocabulary

| Term | Definition | Aliases to avoid |
|------|------------|------------------|
| **Member** | Person row created from Google (`Email`, `Username`). | "user" |
| **Band** | Collaboration group with one Drive folder root. | "artist" when you mean the group |
| **Song** | Metadata + `Url` pointer. | "blob", "file" |
| **Song identifier** | Join of a song to a band. | "playlist item" |
| **Drive pointer** | `gdrive:{fileId}` stored on `Song.Url`. | "Drive URL" when it is only an id |
| **Album** | Catalog of admitted songs on a band. | "container" for audio |
| **Proposal** | Open ask to admit a song; status open / approved / rejected / withdrawn. | "vote" when you mean the decision row |

## Relationships

- **Band** *—* **Member** via **BandMember** (`RoleName`: owner / uploader / member).
- **Band** *—* **Album**; **Album** *—* **Song** via **AlbumTrack** after a proposal is approved.
- **Band** *—* **Song** via **SongIdentifier**.
- **Member** uploads **Song** (`UploadedBy`).
- **GoogleAccount** tokens belong to one **Member**.

## Boundaries / IO

- **Exposes:** `/api/auth/me`, `/api/auth/google/*`, `/api/bands/{id}/songs`, `/api/bands/{id}/albums`, `/api/albums/{id}`, `/api/albums/{id}/proposals`, `/api/albums/{id}/proposals/{id}/reviews`, `/api/bands/{id}/drive/files`, `/api/bands/{id}/drive-folder`, `/api/bands/{id}/invites`, `/api/invites/accept`, `/api/songs/{id}/audio`, `/api/members`.
- **Consumes:** SQL Server; Google OAuth + Drive API; optional SMTP.

## Invariants

- `Song.Url` is a pointer, never embedded bytes.
- Playback supports HTTP Range.
- Drive list/play/link use the signed-in member's token and reject file ids outside the band folder.
- Missing Google credentials must not take down URL song playback.
