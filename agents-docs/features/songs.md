# Songs

> **Area:** songs
> **Status:** Active
> **Last updated:** 2026-09-04

## Overview

Songs are metadata rows (name, version, storage pointer). Audio bytes are not stored in SQL Server. Playback is `GET /api/songs/{id}/audio`.

## Responsibilities

- List a band's songs
- Stream or proxy audio by song id (HTTP Range)
- Does **not** own Drive OAuth (see [Google Drive](google-drive.md))

## Key concepts

- **Song**: metadata + `Url` pointer (`https://...` or `gdrive:{fileId}`)
- **Song identifier**: join of song to band
- **Version**: integer on the song; `PreviousVersion` points at the prior row (0 = none)

## API Endpoint

### Band songs

- **Method:** GET
- **Path:** `/api/bands/{bandId}/songs`
- **Authentication:** None

```json
[{ "id": 1, "name": "Demo Take", "description": "...", "url": "https://...", "version": 1, "previousVersion": 0, "uploadedBy": 1 }]
```

### Play by id

- **Method:** GET
- **Path:** `/api/songs/{id}/audio`
- **Authentication:** None
- **Notes:** `Accept-Ranges: bytes`. Content-Type from upstream or `audio/mpeg`. HTML5 audio uses this URL.
