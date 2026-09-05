# ADR-0001: Record Architectural Decisions

## Status
Accepted

## Context
We need a lightweight way to record architectural decisions so that future agents and engineers can understand *why* the system looks the way it does, not just *what* it does.

## Decision
We use Architecture Decision Records (ADRs) in the Nygard short form. Each ADR lives at `agents-docs/adr/NNNN-slug.md` with a 4-digit zero-padded number, monotonically increasing. The minimum content is a title plus 1–3 sentences each for Context, Decision, and Rationale.

## Rationale
Nygard short form is the lowest-friction format that still captures the *why*. The 3-criteria gate (hard to reverse, surprising without context, genuine trade-offs) keeps the directory from filling with trivia. See `agents-docs/AGENTS_ADRS.md` for the full contract.
