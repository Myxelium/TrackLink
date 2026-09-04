# Members

> **Area:** members
> **Status:** Active
> **Last updated:** 2026-09-04

## Overview

Members are people in a band. The API returns member information for the studio UI session picker and profile strip.

## Responsibilities

- List members and return one member with bands and roles
- Does **not** own Google identity or Drive tokens (see [Google Drive](google-drive.md))

## Key concepts

- **Member**: a person with `Username`, optional `Fullname` / `Image`, and a stable `UserIdentifier` GUID
- **Band member**: join row linking a member to a band
- **Current member**: the studio session, sent as `X-Member-Id`

## API Endpoint

### List

- **Method:** GET
- **Path:** `/api/members`
- **Authentication:** None

```json
[{ "id": 1, "userIdentifier": "...", "username": "demo", "fullname": "Demo User", "image": null }]
```

### Member information

- **Method:** GET
- **Path:** `/api/members/{id}`
- **Authentication:** None

```json
{
  "id": 1,
  "userIdentifier": "...",
  "username": "demo",
  "fullname": "Demo User",
  "image": null,
  "bands": [{ "id": 1, "name": "Kindred", "genre": "indie", "image": null }],
  "roles": [{ "id": 1, "roleName": "Producer", "bandId": 1 }]
}
```
