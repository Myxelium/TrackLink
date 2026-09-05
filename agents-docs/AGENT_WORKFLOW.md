# Agent Workflow & Operating Instructions

These rules apply to **all AI agents** working on this project.

**Token budget (mandatory):**

- Default scope: **`app/` + `api/` + `.github/workflows/`** — see `/AGENTS.md`.
- Prefer path-scoped search; one focused agent; no Bugbot / security / best-of-N unless asked.
- **Handoff > fat chats:** agents cannot open new chats. Write `agents-docs/HANDOFF.md` and ask the user to start a new chat with that file attached.
- **Models:** keep the user’s latest problem-solving model. Cut cost with scope + handoffs, not weaker models.

Do **not** re-read this whole file every turn after the first skim.

---

## Workflow Orchestration

### 1. Interview before implement (default)

- For bugs/features: short interview first (understanding, gaps, choices + recommended default, scope, proof) — then wait. See `/AGENTS.md` and `.cursor/rules/interview-before-fix.mdc`.
- Do not guess past ambiguity; the user controls the implementation choices.
- Skip only when the user opts out (“just fix it”) or an approved handoff already decided.

### 2. Plan mode

- Use plan mode when architecture is unclear or the user asks — the interview often replaces a heavy plan for normal bug fixes.
- Skip long planning essays; prefer bullet choices.

### 3. Subagents sparingly

- Default: one agent.
- Subagents only for true parallel search inside allowed paths.
- Never spawn extra review agents unless the user asks.

### 4. Handoff / short sessions

Triggers: user says handoff / new chat; thread is long with more major work left; switching objectives; blocked on credentials or a product decision.

Action: **overwrite** (never append) `agents-docs/HANDOFF.md` with `Status: active` and short sections. Then stop major work and ask the user to open a new chat.

New chat: if handoff is active, read it first; continue Next steps; do not redo Completed work. If Next steps still need choices, re-interview — don’t invent them.

**When finished:** clear `HANDOFF.md` to `Status: none` with empty sections so the file stays tiny for the next session.

### 5. Self-Improvement Loop

**At session start:** `LESSONS-INDEX.md` only; open matching lesson bodies by tag.

Record a lesson + index line when corrected. Prefer fewer sharp rules (~20).

### 6. CONTEXT.md upkeep

Default: `app/CONTEXT.md` or `api/CONTEXT.md` for the subdomain you touched. See `agents-docs/AGENTS_CONTEXT.md`.

### 7. ADR upkeep

Only when hard-to-reverse + surprising + real trade-offs. Contract: `agents-docs/AGENTS_ADRS.md`.

Write an ADR when all three are true:

1. Hard to reverse
2. Surprising without context
3. Genuine trade-offs were considered

### 8. Verification Before Done (behavior first)

Done = asked functionality works **as the user confirmed in the interview**. Unit green ≠ done for product asks.

### 9. Demand Elegance (Balanced)

One pause for non-trivial design; skip for obvious fixes once the user has chosen a direction.

### 10. Bug fixing (after interview)

Implement the approved plan with evidence in default scope. If root cause is clearly outside scope, say so and ask to expand — don’t silently crawl.

---

## Pull Requests

GitHub: `github.com/Myxelium/TrackLink`. Branch `<type>/<short-description>`; PR with summary + test plan; `Fixes #<n>` / `Relates to #<n>`.
