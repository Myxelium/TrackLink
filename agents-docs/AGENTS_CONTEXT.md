# Agent Instructions: CONTEXT.md & CONTEXT-MAP.md

Domain documentation lives in **`CONTEXT.md`** files co-located with the code they describe:

- **Multi-context repo:** one `CONTEXT.md` per subdomain (`app/CONTEXT.md`, `api/CONTEXT.md`), indexed by `agents-docs/CONTEXT-MAP.md`.

This document defines how agents must detect, document, and maintain domain knowledge as the codebase grows.

> This file is part of the agent instruction infrastructure.
> Do NOT create, delete, or modify this file unless explicitly instructed.

---

## What `CONTEXT.md` is for

A subdomain's `CONTEXT.md` is a **domain artefact**, not an agent-rule file. It captures:

- **Vocabulary** — the bounded-context glossary
- **Relationships** — how the domain terms connect
- **Boundaries / IO** — what this subdomain exposes and consumes
- **Invariants** — rules that always hold
- **Flagged ambiguities** — terms still in dispute

Agent-procedural rules live in `/AGENTS.md` and `agents-docs/ENGINEERING.md` — never in `CONTEXT.md`.

Implementation detail (file paths, request schemas) belongs in `agents-docs/features/<area>.md` — never in `CONTEXT.md`.

## What `CONTEXT-MAP.md` is for

The system-level index of bounded contexts. One row per subdomain.

## CONTEXT Contract (MANDATORY)

### Read at session start

Before working in a subdomain:

1. Read that subdomain's `CONTEXT.md`. If `agents-docs/CONTEXT-MAP.md` exists, start there to locate the right one.
2. If your change couples two subdomains, read both `CONTEXT.md`s.
3. Skip files that don't exist. **Proceed silently**.

### Use the vocabulary verbatim

When your output names a domain concept, use the term as defined in `CONTEXT.md`.

### Flag gaps; don't invent

If the concept you need isn't in the glossary yet, add it or reconsider the name. Don't silently coin a new term.

### Update in the moment

When a trigger fires, update the relevant `CONTEXT.md` in the same turn.
