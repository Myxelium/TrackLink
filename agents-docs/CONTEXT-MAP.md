# Context map

| Context | Purpose | Public surface | CONTEXT.md |
|---------|---------|----------------|------------|
| Product client | Angular studio UI: member session, band songs, playback | `http://localhost:4200` | `app/CONTEXT.md` |
| API | Metadata, member queries, audio stream by song id, Google OAuth/Drive | `http://localhost:5180/api/*` | `api/CONTEXT.md` |

## Relationships

- The **product client** consumes the **API** over `/api` (dev proxy) with `X-Member-Id` for the active member.
- The **API** stores metadata in SQL Server and resolves audio from an HTTP URL or Google Drive file id.
