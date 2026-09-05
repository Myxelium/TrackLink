# Graphify (dev/agent tooling)

Host-only knowledge graph of TrackLink **code**. Not a product dependency. Not a submodule. Does not replace `CONTEXT.md` or `LESSONS.md`.

**Package:** `graphifyy` (double y) on PyPI. **CLI:** `graphify`. The package named `graphify` is the wrong one.

Pinned CLI (discovered at install): see [Installed version](#installed-version) below.

Upstream: https://github.com/Graphify-Labs/graphify

## Install (host machine)

Needs Python 3.10+. Prefer an isolated tool install:

```bash
# recommended
uv tool install graphifyy
# if `graphify` is missing: uv tool update-shell  (then new terminal)

# alternative
pipx install graphifyy
# if `graphify` is missing: pipx ensurepath
```

Do **not** `pip install graphify` (wrong package). Avoid plain `pip install graphifyy` unless you must — PATH / env mixups are common.

**Devcontainer:** this image does not include Python or graphify. Run the CLI on the host (or add Python later — do not assume it is there).

Confirm:

```bash
graphify --version
```

## Extract (code-only)

From the repo root. `.graphifyignore` keeps the corpus in `app/src/`, `api/`, and `agents-docs/`. `--code-only` is local AST (tree-sitter): no API key, no docs/PDF/image pass.

```bash
cd /path/to/TrackLink
graphify extract . --code-only --no-viz
# extract writes graph.json only; the report needs a local cluster pass (no API key):
graphify cluster-only . --no-viz --no-label
```

`--no-viz` skips `graph.html` (often huge). `--no-label` keeps `Community N` names so clustering does not call an LLM.

Commit these:

- `graphify-out/graph.json`
- `graphify-out/GRAPH_REPORT.md`

`manifest.json` is portable and useful for `graphify update`. Ignore locally (already in `.gitignore`):

- `graphify-out/cost.json`
- `graphify-out/cache/`
- `graphify-out/graph.html`
- `graphify-out/.graphify_root` (absolute host path)
- `graphify-out/.graphify_python` (host interpreter, if created)

A code-only extract **skips** markdown in `agents-docs/` (no LLM). The ignore allowlist is there so a later semantic pass can include those docs if someone adds a backend.

## Query

After cheap bootstrap (`AGENTS.md` → handoff → lessons index → CONTEXT.md), for architecture / "where is X wired":

```bash
graphify query "where is ListBandSongs and Drive folder scope wired?"
graphify path "ListBandSongs" "DriveFolderScope" --undirected
graphify explain "ListBandSongs"
```

`ListBandSongs` is ambiguous (API handler vs Angular `TracklinkApi` method). Prefer a repo-relative path or the node id from `query` / `explain`. Directed `path` can miss import-only links — use `--undirected` when that happens.

Do not run Graphify before every Read/Grep. Single-file work stays on CONTEXT.md + scoped search.

## Refresh

After structural `app/` or `api/` changes (new handlers, moved domains, new routes):

```bash
graphify update .
```

Or a full rebuild: `graphify extract . --code-only --no-viz --force` then `graphify cluster-only . --no-viz --no-label`.

No CI job. Optional **local-only** post-commit rebuild (do **not** commit a hook into this repo):

```bash
# on your machine only — not part of the TrackLink tree
graphify hook install
```

`graphify hook install` writes `.git/hooks` on that clone. Uninstall with `graphify hook uninstall` if you do not want it.

## What not to do

- **`graphify cursor install`** — overwrites `.cursor/rules/graphify.mdc` with `alwaysApply: true` and burns tokens every chat. Use the hand-written rule (`alwaysApply: false`).
- **`graphify reflect` into LESSONS** — default `graphify reflect` writes `reflections/LESSONS.md` or can be aimed at a lessons file. It must **never** overwrite `agents-docs/LESSONS.md`. If you reflect, use `--out` somewhere else (or skip). TrackLink lessons stay human-curated.
- **`pip install graphify`** — wrong package.
- **Vendoring** Graphify source into `agents-docs/` or as a git submodule.
- **API keys** — not needed for `--code-only`. A semantic pass over docs/PDFs/images needs a backend key (`ANTHROPIC_API_KEY`, `OPENAI_API_KEY`, `GEMINI_API_KEY`, …) or `--backend ollama`. Do not add those to the repo.
- **App/API dependency** — do not add `graphifyy` to Angular or ASP.NET project files.

## Installed version

Pinned from the host that built `graphify-out/` (2026-09-05):

- CLI / package: `graphify` **0.9.54** (`graphifyy==0.9.54` via `uv tool install graphifyy`)

## Sample query

```bash
graphify query "where is ListBandSongs and Drive folder scope wired for the songs list?"
```

Found (882-node graph): `ListBandSongs` in `api/Handlers/Bands/ListBandSongs.cs` — `Handler.Handle()` calls `.FolderFileIds()` then `.IsVisibleTake()`; `.FolderFileIds()` calls `IGoogleDriveService.ListAudioFilesAsync()`. `DriveFolderScope` / `.IsInsideRoot()` live in `api/Services/DriveFolderScope.cs` and are used from `GoogleDriveService.IsFileInsideFolderAsync()`.

```bash
graphify path "ListBandSongs" "DriveFolderScope" --undirected
```

4 hops: `ListBandSongs` ←contains— `ListBandSongs.cs` —imports→ `api.Services` ←contains— `DriveFolderScope.cs` —contains→ `DriveFolderScope`.
