# Agent Lessons

Full lesson bodies. Session start reads `LESSONS-INDEX.md` and opens only matching entries.

## Lessons

### Agent-launched `dotnet run` dies on shell abort

`[api] [process] [cursor] [upload]`

A Cursor agent foreground `dotnet run` is SIGTERMed when that shell is aborted (`Application is shutting down...`, no ERR/FTL). Album art then looks like a crash: 403 `needs_reauth` is handled, the user re-auths, then :5180 is gone and `ng serve` logs `ECONNREFUSED`. Start with `bash api/start-dev.sh` (`setsid`) so the host outlives the agent turn.

### Album art upload must map Drive failures, not throw

`[api] [albums] [drive] [upload]`

`POST /api/albums/{id}/art` talks to Drive write. A stale read-only token (OAuth widened to full Drive) comes back as 403 / `invalid_grant`, not a process crash. Catch Drive/token/image-decode exceptions in the upload handler, log them, and return JSON `needs_reauth` (403) or `upload_failed` (409). Studio must `catchError` the POST and show the banner — do not leave `artUploading` stuck or read `error.error` on an HTML 500 body.

### Studio "API unreachable" is session load, not invite POST

`[app] [invite] [api] [session]`

`Could not reach the TrackLink API. Start the API on port 5180.` is set only when `GET /api/auth/me` fails in studio `ngOnInit`. Invite create/accept have their own banners. `/api/auth/me` returns 200 even when unsigned, so that banner means the Angular `/api` proxy could not reach `:5180` (process killed by a later `fuser -k` / aborted `dotnet run`, not an invite handler crash). `POST /api/invites/accept` can return 401 with `needsLogin` + `loginUrl`; HttpClient treats that as an error, so the join page must read the 401 body instead of showing a generic accept failure.

### ESLint rewrites a unicode arrow to ASCII

`[app] [lint] [unicode]`

`npm run lint:fix` turned `←` into `<-` in `take-version-label.ts`. Keep the glyph as `\u2190` in source and specs so style fixers cannot replace it.

### Jasmine `toContain` can drop a unicode arrow

`[app] [tests] [unicode]`

`expect(text).toContain('2 ← 1')` failed even when the DOM had `2 ← 1`. Assert `\u2190` on the cell text, or compare a domain helper string.

### Unsigned song list must not dump leftover Drive takes

`[api] [songs] [drive]`

`GET /api/bands/{id}/songs` is public for URL demos. After the folder sandbox, leftover `gdrive:` rows from the old full-Drive scan still sit in SQL. Hide Drive takes unless a signed-in member can confirm the file is inside the band folder. Do not delete the rows unless the user chooses unlink.

### Do not set `ProposalId` before the proposal row exists

`[api] [albums] [ef]`

Admitting a take in the same `SaveChanges` as a new proposal must use the `Proposal` navigation, not `ProposalId = proposal.Id` (still 0). SQL Server then fails `FK_AlbumTrack_Proposal`. InMemory tests hide this.

### Do not `new` a component that uses `input()`

`[app] [angular] [tests]`

`input()` and `output()` need an injection context. `new StudioAlbumWorkComponent()` throws NG0203. Export a plain function for the logic, or create the component with `TestBed`.

### Seed members must not block album admission

`[api] [albums] [session]`

Kindred is seeded with Ada and Kit, who have no Google identity. First Google login becomes owner but leaves those rows on the band. An `all` approval rule that counts every BandMember waits forever. Required voters are members who can actually vote (email / Google subject / Google account). If none exist, fall back to the full roster.

### Angular `@else if (x(); as y)` is invalid

`[app] [angular] [templates]`

`@if (proposal(); as openProposal)` allows `as`. `@else if (album(); as openAlbum)` does not — JIT fails with `"as" expression is only allowed on the primary @if block`. Nest a second `@if` inside `@else`.

### `Google.Apis` is swallowed inside `api.Integrations.Google`

`[api] [csharp] [namespaces]`

A namespace named `api.Integrations.Google` makes `Google.Apis.Drive.v3.Data.File` resolve as `api.Integrations.Google.Apis...` (CS0234). Qualify the SDK type with `global::Google.Apis...`.

### Empty Angular `allowedHosts` rejects localhost in SSR

`[app] [ssr] [dev]`

`angular.json` `build.options.security.allowedHosts: []` makes `ng serve` return 400 `URL with hostname "localhost" is not allowed`. Include `localhost` and `127.0.0.1` for local studio + OAuth return to `:4200`. Binding `--host 0.0.0.0` only allows that Host header until the allowlist is set.

### User-secrets keys use colons, not env-var underscores

`[api] [oauth] [secrets]`

`dotnet user-secrets set Google__ClientId` stores a flat key that does **not** bind to `Google:ClientId`. Status stays `configured: false`. Use `Google:ClientId` and `Google:ClientSecret`. Env vars still use `Google__ClientId`.

### Audio playback must honor HTTP Range so the player can seek

`[api] [audio]`

`GET /api/songs/{id}/audio` is consumed by an HTML5 `<audio>` element. Browsers send `Range` for seeking. Return a seekable stream (or proxy the upstream Range) and let `File(..., enableRangeProcessing: true)` emit `206`. Do not buffer the entire file into a string.

### Google Drive bytes never belong in SQL Server; store file id or URL only

`[storage] [drive]`

TrackLink metadata (name, version, band link) lives in SQL Server. Audio bytes live in Drive or at an HTTP URL. Persist `gdrive:{fileId}` or a https URL on `Song.Url`. The play endpoint resolves that pointer at request time.
