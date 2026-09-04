# Agent Lessons

Full lesson bodies. Session start reads `LESSONS-INDEX.md` and opens only matching entries.

## Lessons

### Audio playback must honor HTTP Range so the player can seek

`[api] [audio]`

`GET /api/songs/{id}/audio` is consumed by an HTML5 `<audio>` element. Browsers send `Range` for seeking. Return a seekable stream (or proxy the upstream Range) and let `File(..., enableRangeProcessing: true)` emit `206`. Do not buffer the entire file into a string.

### Google Drive bytes never belong in SQL Server; store file id or URL only

`[storage] [drive]`

TrackLink metadata (name, version, band link) lives in SQL Server. Audio bytes live in Drive or at an HTTP URL. Persist `gdrive:{fileId}` or a https URL on `Song.Url`. The play endpoint resolves that pointer at request time.
