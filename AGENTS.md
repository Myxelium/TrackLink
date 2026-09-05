# AGENTS.md

Keep this file small. Detail lives in linked docs — load those **only when the task needs them**.

## Session handoff (new chats)

Agents **cannot** open a new Cursor chat. To reset context:

1. **Overwrite** `agents-docs/HANDOFF.md` entirely (`Status: active`) — never append. See `.cursor/rules/handoff.mdc`.
2. Ask the user to start a **new chat** and attach `@agents-docs/HANDOFF.md`.

**At session start:** if `Status: active`, read it first and continue Next steps. Do not redo Completed work.

**When the task is finished:** clear the handoff — set `Status: none` and empty all sections (same short template). Never leave a growing archive in `HANDOFF.md`.

## Interview before implement (user in control)

Before changing product code for a bug or feature:

1. Read the ask (cheap orientation only — not a monorepo dig).
2. Post a **short interview**: your understanding, gaps, A/B/C choices with a recommended default, proposed scope, how you’ll prove done.
3. **Wait** for the user’s choices. Do not guess past ambiguity.
4. Implement only what they approved.

Skip only if they say “just fix it” / “no interview”, or an active handoff already has approved decisions and they said continue. See `.cursor/rules/interview-before-fix.mdc`.

## Default work scope (token fence)

Unless the user **explicitly** expands scope, stay inside:

- `app/` (Angular product client)
- `api/` (ASP.NET Core API)
- CI: `.github/workflows/`
- Agent/docs as needed: this file, `agents-docs/`, `app/CONTEXT.md`, `api/CONTEXT.md`

**Still out of scope by default** (do not search/read/edit unless the user names them):

- Root noise: `app/dist/`, `app/node_modules/`, `api/bin/`, `api/obj/`

Search with path filters. Prefer `app/src/app/domains/<name>/` over repo-wide greps.

## Session start (cheap bootstrap)

1. Skim this file.
2. If handoff `Status: active` → read `agents-docs/HANDOFF.md`.
3. Open `agents-docs/LESSONS-INDEX.md` only — match tags; open matching bodies in `LESSONS.md`.
4. Read `app/CONTEXT.md` for client work; `api/CONTEXT.md` for API work.
5. Other docs **on demand** only.
6. If `graphify-out/graph.json` exists and the ask is architecture / cross-file, query the graph **after** this bootstrap (`agents-docs/GRAPHIFY.md`). Graphify does not replace CONTEXT.md / LESSONS.

**Models:** use the latest problem-solving model the user selected. Save tokens with **scope, handoffs, and short chats** — not by silently downgrading model quality.

**Do not auto-read:** `ENGINEERING.md`, `AGENTS_FEATURES.md`, `FEATURES.md`, `CONTEXT-MAP.md`, full `AGENT_WORKFLOW.md`, feature docs, ADRs — unless needed.

On-demand: `agents-docs/AGENT_WORKFLOW.md`, `AGENTS_FEATURES.md`, `FEATURES.md`, `ENGINEERING.md`, `AGENTS_CONTEXT.md`, `AGENTS_ADRS.md`.

---

TrackLink: remote-band album assembly. Metadata in SQL Server; audio bytes live in cloud storage (Google Drive or a URL). Default surface: Angular client (`app/`) + ASP.NET API (`api/`).

## CRITICAL — Done means the asked behavior works

**Unit/spec green is support, not done.**

1. Restate acceptance in one sentence.
2. Prove the behavior (user-visible path, focused test at the right level, or explicit manual check).
3. Prefer a regression that fails if the asked behavior regresses.

### Test-backed development (balanced)

For domain/logic: failing behavior-level test → minimal fix → green.
Skip full red-green for docs/copy/agent text, formatting, trivial wiring already covered higher up.

**Do not:** ship implementation-shaped mocks as the feature; stop at unit-green for product asks; run full-repo tests on every tiny change — targeted specs first.

### Lint / type correctness (scoped)

1. Targeted specs under `app/` (Karma/Jasmine) for UI logic you changed.
2. Auto-fix style first: `npm run lint:fix` from `app/` (junolint / ESLint). Do **not** hand-edit formatting/import-sort/eslint-fixable issues — re-run `lint:fix`. Then confirm clean with `npm run lint` only if you need a no-write check.
3. Do not paste entire lint logs into the chat — fix via `lint:fix` / minimal code changes for non-auto issues.
4. `npm run build` in `app/` when client types/templates could break; `dotnet build` in `api/` when the API changed.

### Feature docs

Internal domain changes → `app/src/app/domains/<name>/README.md`.
HTTP/storage contract changes → `agents-docs/features/<slug>.md` when that contract actually changed.

## Completion checklist

- [ ] Interview completed (or user opted out); implemented only approved choices
- [ ] Asked behavior proven (not only unit tests green)
- [ ] Stayed in scope (`app/` + `api/` + CI) unless user expanded it
- [ ] Appropriate targeted tests for logic changes
- [ ] Lint via `npm run lint:fix` in `app/` (not hand-fixed style); build only touched packages
- [ ] Docs only if contracts changed
- [ ] Lesson + index entry if corrected this session
- [ ] If the thread is long and work remains: overwrite `HANDOFF.md` and ask user for a new chat
- [ ] If work from an active handoff is finished: clear `HANDOFF.md` to `Status: none` (empty sections)
- [ ] PR when requesting merge (`Fixes #<n>` / `Relates to #<n>`)
