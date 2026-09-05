# Agent Instructions: Feature Areas & Documentation

All feature documentation lives under **`agents-docs/features/`**:

- **Area-level docs** (`agents-docs/features/<area>.md`): concept-first overview of a feature area
- Use `agents-docs/features/feature-template.md` when creating a new area

This document defines how agents must detect, document, and maintain feature knowledge as the codebase grows.

> This file is part of the agent instruction infrastructure.
> Do NOT create, delete, or modify this file unless explicitly instructed.

---

## What is a feature area?

A feature area is a named concept that appears in API routes or domain services and represents a coherent responsibility.

---

## Feature Documentation Contract (MANDATORY)

### When to create or update area-level docs

- New feature area introduced → create `agents-docs/features/<slug>.md` and add to `agents-docs/FEATURES.md` (alphabetical).
- Changes to **responsibilities, boundaries, workflows, or high-level behavior** → update the relevant area doc in the same task.

### When an existing feature area changes

If the HTTP contract, storage pointer format, or auth flow changes, update the area doc in the same task.
