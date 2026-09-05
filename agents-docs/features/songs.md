# Songs

> **Area:** songs
> **Status:** Active
> **Last updated:** 2026-09-05

## Overview

Songs are metadata rows (name, version, storage pointer). Audio bytes are not stored in SQL Server. Playback is `GET /api/songs/{id}/audio`.

## Responsibilities

- List a band's songs
- Stream or proxy audio by song id (HTTP Range)
- Does **not** own Drive OAuth (see [Google Drive](google-drive.md))

## Key concepts

- **Song**: metadata + `Url` pointer (`https://...` or `gdrive:{fileId}`)
- **Song identifier**: join of song to band
- **Version**: integer on the song; `PreviousVersion` is the prior song id (0 = none)
- **ContentMd5 / SourceModifiedAt**: optional Drive checksum and file modified time, stored when Google returns them

## API Endpoint

### Band songs

- **Method:** GET
- **Path:** `/api/bands/{bandId}/songs`
- **Authentication:** None. Session cookie is used when present so Drive takes outside the band folder can be omitted.
- **Query:** `q` (optional). Case-insensitive match on song name or description. Blank or omitted `q` returns the full visible catalog. Search stays on this band's SQL rows — it does not crawl Drive.
- **Notes:** URL takes always list. `gdrive` takes list only when the caller is signed in, the band folder is set, and the file sits under that folder. SQL rows stay; leftover full-Drive links are hidden, not deleted. Hidden leftovers stay hidden even when `q` matches their name.

```json
[{ "id": 1, "name": "Demo Take", "description": "...", "version": 1, "previousVersion": 0, "uploadedBy": 1, "storageKind": "url", "contentMd5": null, "sourceModifiedAt": null }]
```

### Link a Drive file

- **Method:** POST
- **Path:** `/api/bands/{bandId}/songs`
- **Authentication:** Session cookie; owner or uploader. File must sit under the band Drive folder.
- **Notes:** Stores `gdrive:{driveFileId}` on `Song.Url`. Does not copy bytes into SQL Server. The same Drive file id on this band returns the existing song. A new file with the same name becomes the next version (`PreviousVersion` = prior song id). When Drive returns them, the row also stores `contentMd5` and `sourceModifiedAt`.

```json
{ "driveFileId": "1abc", "name": "bottleneck.mp3" }
```

```json
{ "id": 8, "name": "bottleneck.mp3", "description": "Linked from Google Drive", "uploadedBy": 1, "version": 2, "previousVersion": 7, "storageKind": "gdrive", "contentMd5": "abc123", "sourceModifiedAt": "2026-09-05T08:00:00Z" }
```

### Play by id

- **Method:** GET
- **Path:** `/api/songs/{id}/audio`
- **Authentication:** None
- **Notes:** `Accept-Ranges: bytes`. Content-Type from upstream or `audio/mpeg`. HTML5 audio uses this URL.
