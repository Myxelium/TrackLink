# Agent Instructions: Architecture Decision Records (ADRs)

Architectural decisions live in **`agents-docs/adr/`** as numbered Markdown files (`NNNN-slug.md`).

This document defines how agents must detect, document, and maintain architectural decisions as the codebase grows.

> This file is part of the agent instruction infrastructure.
> Do NOT create, delete, or modify this file unless explicitly instructed.

---

## What an ADR is

A short record of an architectural decision. Nygard short form:

- Title and number (`ADR-NNNN: <slug>`).
- Required: 1–3 sentences each covering **Context**, **Decision**, and **Rationale**.
- Conventional: `Status` (`Accepted`, or `Superseded by ADR-MMMM`).

See `agents-docs/adr/0001-record-architectural-decisions.md` for the canonical example.

---

## ADR Contract (MANDATORY)

### When to write an ADR

The 3-criteria gate lives in `agents-docs/AGENT_WORKFLOW.md` § 7 ADR upkeep. Write an ADR only when the decision is **hard to reverse**, **surprising without context**, and the **result of genuine trade-offs**.

### Read before crossing decision boundaries

Before non-trivial changes in an area, scan `agents-docs/adr/`. If your work would contradict an existing ADR, surface it explicitly.

### Write the ADR in the same turn as the decision

When the 3-criteria gate is met, write the ADR before reporting the task done.

### Numbering

Scan `agents-docs/adr/` for the highest existing number; the new ADR is `NNNN+1`. Use 4-digit zero-padded numbers. Slugs are kebab-case.
