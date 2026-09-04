---
name: playwright-e2e
description: >
  Write and run Playwright E2E tests for TrackLink (Angular studio + ASP.NET API).
  Use when: "E2E test", "Playwright", "end-to-end", "browser test", "test playback",
  "test first page", "integration test".
---

# Playwright E2E Testing — TrackLink

Playwright is not wired in this repo yet. If you add it, put tests under `e2e/` at the repo root.

## Until then

Prove UI stories in the Cursor browser (or `curl` + `ng serve`):

1. First page loads TrackLink chrome (not the Angular CLI placeholder).
2. Member information renders from `GET /api/members/{id}`.
3. Band songs render from `GET /api/bands/{id}/songs`.
4. Play uses `/api/songs/{id}/audio` (audio element can start).

Do not add a full Playwright stack unless the user asks.
