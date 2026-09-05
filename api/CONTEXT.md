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
| **Inclusion vote** | In / out / abstain on an admitted album take. Does not admit or drop the track. | "approve" when you mean a proposal decision |
| **Name vote** | Straw poll for an album title or an admitted song title. Does not rename. | "approve" when you mean a proposal decision |
| **Order vote** | Ranked list of admitted album takes. Lock writes that consensus onto track `SortOrder`. | "inclusion" when you mean in/out |
| **Art vote** | Straw poll for a Drive file id under the band folder. Lock writes the winner onto `ArtDriveFileId`. | "cover upload" when it is only a vote |

## Relationships

- **Band** *—* **Member** via **BandMember** (`RoleName`: owner / uploader / member).
- **Band** *—* **Album**; **Album** *—* **Song** via **AlbumTrack** after a proposal is approved.
- **Band** *—* **Song** via **SongIdentifier**.
- **Member** uploads **Song** (`UploadedBy`).
- **GoogleAccount** tokens belong to one **Member**.

## Boundaries / IO

- **Writes:** rolling daily logs at `logs/tracklink-.log` under the API content root (`api/logs/` when run from this project). Dev host: `bash api/start-dev.sh` (own session). A foreground agent `dotnet run` is SIGTERMed when that shell aborts.
- **Exposes:** `/api/auth/me`, `/api/auth/google/*`, `/api/bands/{id}/songs` (`q` filters name/description on this band), `/api/bands/{id}/albums`, `/api/albums/{id}`, `/api/albums/{id}/art` (`POST` multipart `file` uploads a cover candidate into the band folder; `GET` streams the applied cover; `?fileId=` for a candidate thumb), `/api/albums/{id}/inclusion-votes`, `/api/albums/{id}/name-votes`, `/api/albums/{id}/order-votes`, `/api/albums/{id}/order-lock`, `/api/albums/{id}/art-votes`, `/api/albums/{id}/art-lock`, `/api/albums/{id}/proposals`, `/api/albums/{id}/proposals/{id}/reviews`, `/api/bands/{id}/drive/files` (`kind=audio` default, `kind=image` for covers), `/api/bands/{id}/drive-folder`, `/api/bands/{id}/invites`, `/api/invites/accept`, `/api/songs/{id}/audio`, `/api/members`.
- **Consumes:** SQL Server; Google OAuth + Drive API; optional SMTP.

## Invariants

- `Song.Url` is a pointer, never embedded bytes.
- Playback supports HTTP Range.
- Drive list/play/link use the signed-in member's token and reject file ids outside the band folder.
- Missing Google credentials must not take down URL song playback.
- Band song search (`q`) is SQL name/description on that band only — never a Drive crawl.
- Inclusion votes (in / out / abstain) never admit or drop an album track.
- Name votes never rename the album or song; they only persist a candidate and tally.
- Order votes rank admitted takes only. Lock (owners/uploaders) writes consensus onto `AlbumTrack.SortOrder`. A locked album rejects new rankings.
- Art votes pick a Drive file id inside the band folder. The file must be an image (`image/*` via Drive metadata). Lock (owners/uploaders) writes consensus onto `Album.ArtDriveFileId`. A locked album rejects new art votes and new cover uploads. Unlock reopens voting without changing the cover. `POST /api/albums/{id}/art` uploads an image into the band folder as a candidate and does not apply the cover. A size outside 1600–3000 on either side returns `warning` and still succeeds. Drive write/token failures return JSON (`needs_reauth` / `upload_failed`) and must not take down the process. `GET /api/albums/{id}/art` streams the applied image; `?fileId=` streams a candidate. Both stay inside the band folder and reject non-images. If the applied cover is gone from Drive, album GET and the applied-cover preview clear `ArtDriveFileId`.
