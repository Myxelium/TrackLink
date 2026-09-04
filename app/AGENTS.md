# Product client guidelines

This package is the Angular studio UI for TrackLink.

## Before you edit

- From `app/`: `npm start`, `npm run build`, `npm test`, `npm run lint`, `npm run lint:fix`.
- Read `CONTEXT.md` before adding domain terms.
- Junolint owns TS + template layout — do not hand-fix auto-fixable lint.

## Architecture

- Keep models and rules in `src/app/domains/<name>/`.
- Keep HTTP adapters in `src/app/core/`.
- Keep app shell / first page in `src/app/features/`.
- Outside a domain, import from that domain's `index.ts` when it exists.

## Before you finish

- Prove the asked UI path in the browser (or the closest substitute).
- Run `npm run lint:fix` on touched templates/TS.
