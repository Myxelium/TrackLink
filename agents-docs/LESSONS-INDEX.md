# Agent Lessons — Index

**Session start:** read this file only. Match tags to the task. Open the matching lesson body in `agents-docs/LESSONS.md` — do **not** load every lesson.

When adding a lesson: append the full entry near the top of `LESSONS.md` (under `## Lessons`) **and** add one bullet here.

Tags help grepping: `rg '\\[api\\]' agents-docs/LESSONS-INDEX.md`

## Index

- Jasmine `toContain` can drop a unicode arrow — `[app] [tests] [unicode]`
- Unsigned song list must not dump leftover Drive takes — `[api] [songs] [drive]`
- Do not set `ProposalId` before the proposal row exists — `[api] [albums] [ef]`
- Do not `new` a component that uses `input()` — `[app] [angular] [tests]`
- Seed members must not block album admission — `[api] [albums] [session]`
- Angular `@else if (x(); as y)` is invalid — nest `@if` — `[app] [angular] [templates]`
- `Google.Apis` is swallowed inside `api.Integrations.Google` — `[api] [csharp] [namespaces]`
- Empty Angular `allowedHosts` rejects localhost in SSR — `[app] [ssr] [dev]`
- User-secrets keys use colons, not env-var underscores — `[api] [oauth] [secrets]`
- Audio playback must honor HTTP Range so the player can seek — `[api] [audio]`
- Google Drive bytes never belong in SQL Server; store file id or URL only — `[storage] [drive]`
