# Product client (app)

Owns the user-facing Angular studio: member session, band song list, and playback against the TrackLink API.

> See `agents-docs/AGENTS_CONTEXT.md` for the contract.

## Vocabulary

| Term | Definition | Aliases to avoid |
|------|------------|------------------|
| **Member** | A person in the band; selected in the studio session. | "user", "account" (Google is separate) |
| **Band** | A collaboration group that owns a song catalog. | "team", "group" |
| **Song** | Metadata + storage pointer for one take/version. | "track" when you mean the metadata row |
| **Current member** | The member id stored in the browser and sent as `X-Member-Id`. | "logged in user" |
| **Play URL** | `GET /api/songs/{id}/audio` — never bind `<audio>` to a raw Drive link from the client. | "file URL", "mp3 href" |

## Relationships

- A **Member** belongs to zero or more **Bands**.
- A **Band** has many **Songs** via song identifiers.
- The **Current member** is a client-only session over a **Member** row.

## Boundaries / IO

- **Exposes:** SPA at `:4200` (dev) with `/api` proxied to the API.
- **Consumes:** `/api/members`, `/api/members/{id}`, `/api/bands/{id}/songs`, `/api/songs/{id}/audio`, `/api/auth/google/*`.

## Invariants

- Audio playback always goes through the API play endpoint so Range and Drive tokens stay on the server.
- Do not read `localStorage` during SSR; gate with the browser platform.

## Flagged ambiguities

- Google OAuth is optional until Cloud credentials exist; the first page must still play URL-backed demo songs.
