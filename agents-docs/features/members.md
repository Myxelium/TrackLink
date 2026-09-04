# Members

> **Area:** members
> **Status:** Active
> **Last updated:** 2026-09-04

## Overview

A Member is a TrackLink person. Google login creates or updates that row. The studio session is the signed-in member cookie, not a picker.

## Responsibilities

- List members and return one member with bands and roles
- Google identity and Drive tokens live on the member (see [Google Drive](google-drive.md))

## Key concepts

- **Member**: `Username`, optional `Fullname` / `Image` / `Email` / `GoogleSubject`, and a stable `UserIdentifier` GUID
- **Band member**: join row with `RoleName` (`owner`, `uploader`, `member`)
- **Current member**: cookie `tracklink.sid` after Google login

## API Endpoint

### Session

See [Google Drive](google-drive.md) `GET /api/auth/me`.

### List

- **Method:** GET
- **Path:** `/api/members`
- **Authentication:** None (used for the unsigned demo catalog)

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
  "email": "ada@example.com",
  "image": null,
  "bands": [{
    "id": 1,
    "name": "Kindred",
    "genre": "indie",
    "image": null,
    "driveFolderId": null,
    "driveFolderName": null,
    "myRole": "owner",
    "isOwner": true
  }],
  "roles": [{ "id": 1, "roleName": "Producer", "bandId": 1 }]
}
```
