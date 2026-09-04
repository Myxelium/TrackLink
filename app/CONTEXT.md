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

## Relationships

- A **Member** belongs to zero or more **Bands**.
- A **Band** has many **Songs** via song identifiers.
- The **Current member** is a cookie session over a **Member** row created at Google login.

## Boundaries / IO

- **Exposes:** SPA at `:4200` (dev) with `/api` proxied to the API.
- **Consumes:** `/api/auth/me`, `/api/auth/google/*`, `/api/bands/{id}/songs`, `/api/bands/{id}/albums`, `/api/albums/{id}`, `/api/albums/{id}/proposals`, `/api/albums/{id}/proposals/{id}/reviews`, `/api/bands/{id}/drive/files`, `/api/bands/{id}/drive-folder`, `/api/bands/{id}/invites`, `/api/invites/accept`, `/api/songs/{id}/audio`.

## Invariants

- Audio playback always goes through the API play endpoint so Range and Drive tokens stay on the server.
- Do not read `localStorage` during SSR; gate with the browser platform.
- Drive lists and links stay inside the owner-picked band folder.

## Flagged ambiguities

- Unsigned studio still plays URL-backed demo songs from the seed catalog.
