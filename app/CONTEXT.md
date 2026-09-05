# Product client (app)

Owns the user-facing Angular studio: Google session, band song list, album desk, Drive folder picker, invites, and playback against the TrackLink API.

> See `agents-docs/AGENTS_CONTEXT.md` for the contract.

## Vocabulary

| Term | Definition | Aliases to avoid |
|------|------------|------------------|
| **Member** | A person created from Google login. | "account" when you mean the TrackLink row |
| **Band** | A collaboration group that owns a song catalog and one Drive folder. | "team", "group" |
| **Song** | Metadata + storage pointer for one take/version. | "track" when you mean the metadata row |
| **Current member** | The signed-in Google session (`GET /api/auth/me`). | "picked member" |
| **Play URL** | `GET /api/songs/{id}/audio` — never bind `<audio>` to a raw Drive link from the client. | "file URL", "mp3 href" |
| **Album** | A catalog of admitted takes for one band. | "playlist", "release" when you mean the SQL row |
| **Proposal** | A PR-like ask to put a band song on an album. | "vote" when you mean admission |
| **Inclusion vote** | In / out / abstain straw poll on an admitted take. | "approve" when you mean a proposal decision |
| **Name vote** | Straw poll for an album title or an admitted song title. Does not rename. | "approve" when you mean a proposal decision |
| **Order vote** | Ranked list of admitted album takes. Lock applies consensus as the displayed track order. | "inclusion" when you mean in/out |
| **Art vote** | Straw poll for a Drive file id under the band folder. Apply writes the winner as the album cover. | "cover upload" when it is only a vote |

## Relationships

- A **Member** belongs to zero or more **Bands**.
- A **Band** has many **Songs** via song identifiers.
- The **Current member** is a cookie session over a **Member** row created at Google login.

## Boundaries / IO

- **Exposes:** SPA at `:4200` (dev) with `/api` proxied to the API.
- **Consumes:** `/api/auth/me`, `/api/auth/google/*`, `/api/bands/{id}/songs` (`q` filters name/description), `/api/bands/{id}/albums`, `/api/albums/{id}`, `/api/albums/{id}/art` (`POST` multipart `file` for a cover candidate; `GET` applied cover; `?fileId=` for a candidate thumb), `/api/albums/{id}/inclusion-votes`, `/api/albums/{id}/name-votes`, `/api/albums/{id}/order-votes`, `/api/albums/{id}/order-lock`, `/api/albums/{id}/art-votes`, `/api/albums/{id}/art-lock`, `/api/albums/{id}/proposals`, `/api/albums/{id}/proposals/{id}/reviews`, `/api/bands/{id}/drive/files` (`kind=image` for covers; default audio for takes), `/api/bands/{id}/drive-folder`, `/api/bands/{id}/invites`, `/api/invites/accept`, `/api/songs/{id}/audio`.

## Invariants

- Audio playback always goes through the API play endpoint so Range and Drive tokens stay on the server.
- Do not read `localStorage` during SSR; gate with the browser platform.
- Drive lists, links, and the Takes catalog stay inside the owner-picked band folder.
- Name votes are a straw poll; they do not rename the album or song from the desk.
- Order votes rank admitted takes. Locking writes that consensus as the album track order.
- Art votes pick a Drive file id inside the band folder. The art poll lists `kind=image` files; the take picker stays audio-only. Any member can upload a local image to the band folder as a candidate (`POST /api/albums/{id}/art`); that does not apply the cover. The desk shows the API size warning when a side is outside 1600–3000. Applying writes that consensus as the album cover. The desk previews the applied cover from `/api/albums/{id}/art` and candidate thumbs from `/api/albums/{id}/art?fileId=`. A typed non-image id is rejected. A Drive 404 for the applied cover clears it on album GET.

## Flagged ambiguities

- Unsigned studio still plays URL-backed demo songs from the seed catalog.
