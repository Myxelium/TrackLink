# Albums

> **Area:** albums
> **Status:** Active
> **Last updated:** 2026-09-04

## Overview

A band album is a catalog of admitted takes. Audio bytes are never stored on the album. Art may later be a Drive file id under the band folder.

## Responsibilities

- Create, rename, archive, and list albums on a band
- Hold accepted tracks in order
- Store the approval rule used to admit takes
- Does **not** own playback or Drive OAuth

## Key concepts

- **Album**: named catalog on one band (`all` or `owner_uploaders` approval)
- **Album track**: a song already admitted, with sort order
- **Proposal**: see [Proposals](proposals.md)

## API Endpoint

### List

- **Method:** GET
- **Path:** `/api/bands/{bandId}/albums`
- **Authentication:** Session cookie; band member

```json
[{ "id": 2, "bandId": 9, "name": "First Light", "archived": false, "approvalRule": "all", "trackCount": 0, "openProposalCount": 1 }]
```

### Create

- **Method:** POST
- **Path:** `/api/bands/{bandId}/albums`
- **Authentication:** Session cookie; owner or uploader

```json
{ "name": "First Light", "approvalRule": "all" }
```

### Open / update

- **Method:** GET / PATCH
- **Path:** `/api/albums/{albumId}`
- **Authentication:** Session cookie; band member to read; owner or uploader to patch

```json
{ "name": "First Light", "archived": false, "approvalRule": "owner_uploaders" }
```
