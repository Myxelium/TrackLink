# Proposals

> **Area:** proposals
> **Status:** Active
> **Last updated:** 2026-09-04

## Overview

A proposal is a pull-request-like ask to put an existing band song on an album. Creating it does not publish the take. Approval votes are per member and replace the prior vote from that member.

## Responsibilities

- Open / withdraw a proposal for a band song
- Record approve or reject
- Admit the song onto the album when the album rule is met
- Reject Drive-backed songs whose file sits outside the band folder
- Timestamped review comments on an open or rejected proposal
- Does **not** own album name / art / order votes

## Key concepts

- **Status**: `open`, `approved`, `rejected`, `withdrawn`
- **Rule `all`**: every member who can actually vote (Google/email identity) must approve
- **Rule `owner_uploaders`**: every owner and uploader who can vote must approve
- **Proposer**: opening a proposal records that member’s approve
- **Waiting on**: required members who have not approved

## API Endpoint

### Open

- **Method:** POST
- **Path:** `/api/albums/{albumId}/proposals`
- **Authentication:** Session cookie; owner or uploader

```json
{ "songId": 4 }
```

### Read

- **Method:** GET
- **Path:** `/api/albums/{albumId}/proposals/{proposalId}`
- **Authentication:** Session cookie; band member

### Decide

- **Method:** POST
- **Path:** `/api/albums/{albumId}/proposals/{proposalId}/decisions`
- **Authentication:** Session cookie; band member

```json
{ "decision": "approve" }
```

### Withdraw

- **Method:** POST
- **Path:** `/api/albums/{albumId}/proposals/{proposalId}/withdraw`
- **Authentication:** Session cookie; proposer or owner

### Review

- **Method:** POST
- **Path:** `/api/albums/{albumId}/proposals/{proposalId}/reviews`
- **Authentication:** Session cookie; band member
- Open, rejected, and approved proposals; not withdrawn

```json
{ "startMs": 12500, "endMs": 18300, "body": "Snare is late" }
```

- **Method:** DELETE
- **Path:** `/api/albums/{albumId}/proposals/{proposalId}/reviews/{reviewId}`
- **Authentication:** Session cookie; author or band owner

Proposal reads include `reviews` ordered by start, then created time. Clicking a review seeks `/api/songs/{id}/audio` to `startMs`.
