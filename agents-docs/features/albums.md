# Albums

> **Area:** albums
> **Status:** Active
> **Last updated:** 2026-09-05 (art candidate upload + stale cover clear)

## Overview

Albums are catalogs of admitted takes. Proposals admit a song. Inclusion votes are a separate in / out / abstain poll on takes already on the album. Name votes are a straw poll for album and song titles and do not rename. Order votes are a ranked list of admitted takes; lock writes the consensus onto the album track order. Art votes are a Drive-file poll; lock writes the winner onto the album cover.

## Inclusion vote

- **Method:** PUT
- **Path:** `/api/albums/{albumId}/inclusion-votes`
- **Authentication:** Session cookie; any band member.
- **Notes:** Upserts one row on the existing `Vote` table (`AlbumId` + `Choice`). The song must already be an album track. `out` does not remove the track. `in` does not admit a proposed-only song. Archived albums reject new votes. Blank `q` is unrelated (song search).

```json
{ "songId": 4, "choice": "in" }
```

`choice` is `in`, `out`, or `abstain`.

```json
{
  "id": 2,
  "tracks": [
    {
      "id": 3,
      "songId": 4,
      "songName": "Night Shift",
      "inclusion": { "myChoice": "in", "inCount": 1, "outCount": 0, "abstainCount": 0 }
    }
  ]
}
```

Album `GET /api/albums/{id}` includes the same `inclusion` tally on each track.

## Name vote

- **Method:** PUT
- **Path:** `/api/albums/{albumId}/name-votes`
- **Authentication:** Session cookie; any band member.
- **Notes:** Straw poll on the existing `Vote` table (`Kind` is `album_name` or `song_name`, `Subject` is the candidate). One ballot per member per contest. Proposing a title casts that member's vote for it. Does **not** rename the album or song. Song titles require an admitted track. Archived albums reject new votes. Distinct from inclusion and from proposal approve/reject.

Album title:

```json
{ "name": "Midnight Sun" }
```

Song title:

```json
{ "songId": 4, "name": "Night Shift (radio)" }
```

`GET /api/albums/{id}` returns the same contests:

```json
{
  "id": 2,
  "name": "First Light",
  "names": {
    "myName": "Midnight Sun",
    "candidates": [
      { "name": "Midnight Sun", "voteCount": 1, "isMine": true },
      { "name": "First Light", "voteCount": 0, "isMine": false }
    ]
  },
  "tracks": [
    {
      "songId": 4,
      "songName": "Night Shift",
      "names": {
        "myName": "Night Shift (radio)",
        "candidates": [
          { "name": "Night Shift (radio)", "voteCount": 1, "isMine": true },
          { "name": "Night Shift", "voteCount": 0, "isMine": false }
        ]
      }
    }
  ]
}
```

## Order vote

- **Method:** PUT
- **Path:** `/api/albums/{albumId}/order-votes`
- **Authentication:** Session cookie; any band member.
- **Notes:** Upserts one `Vote` row per admitted take (`Kind` is `order`, `Choice` is the 1-based rank). The body must be a complete permutation of current album tracks. Proposed-only songs are rejected. Archived albums reject new votes. Distinct from inclusion and name polls. Does not change `SortOrder` until lock.

```json
{ "songIds": [7, 4] }
```

## Order lock

- **Method:** PUT
- **Path:** `/api/albums/{albumId}/order-lock`
- **Authentication:** Session cookie; owners and uploaders (`CanManageAlbums`).
- **Notes:** `{ "locked": true }` freezes rankings and writes consensus (average rank, then current `SortOrder`, then song id) onto `AlbumTrack.SortOrder`. `{ "locked": false }` reopens voting without rewriting order. A locked album rejects further `order-votes`. `GET /api/albums/{id}` returns tracks in `SortOrder` plus the order contest.

```json
{ "locked": true }
```

```json
{
  "id": 2,
  "order": {
    "locked": true,
    "voteCount": 1,
    "mySongIds": [7, 4]
  },
  "tracks": [
    {
      "songId": 7,
      "songName": "Dawn Chorus",
      "sortOrder": 1,
      "order": { "myRank": 1, "averageRank": 1, "consensusPosition": 1 }
    },
    {
      "songId": 4,
      "songName": "Night Shift",
      "sortOrder": 2,
      "order": { "myRank": 2, "averageRank": 2, "consensusPosition": 2 }
    }
  ]
}
```

## Art vote

- **Method:** PUT
- **Path:** `/api/albums/{albumId}/art-votes`
- **Authentication:** Session cookie; any band member.
- **Notes:** Upserts one `Vote` row (`Kind` is `art`, `Subject` is the Drive file id). The file must stay inside the band Drive folder and be an image (Drive mime `image/*`, same sandbox as list/play/link). A typed non-image or a file outside the folder is rejected and does not write a vote. Archived albums and locked art contests reject new votes. Distinct from inclusion, name, and order polls. Does not set `ArtDriveFileId` until lock.

```json
{ "driveFileId": "cover-dawn" }
```

## Art lock

- **Method:** PUT
- **Path:** `/api/albums/{albumId}/art-lock`
- **Authentication:** Session cookie; owners and uploaders (`CanManageAlbums`).
- **Notes:** `{ "locked": true }` freezes art votes and writes consensus (most votes, then the current cover, then file id) onto `Album.ArtDriveFileId`. `{ "locked": false }` reopens voting without rewriting the cover. A locked album rejects further `art-votes`. `GET /api/albums/{id}` returns the art contest.

```json
{ "locked": true }
```

```json
{
  "id": 2,
  "art": {
    "locked": true,
    "appliedDriveFileId": "cover-dawn",
    "voteCount": 1,
    "myDriveFileId": "cover-dawn",
    "candidates": [
      { "driveFileId": "cover-dawn", "voteCount": 1, "isMine": true }
    ]
  }
}
```

## Art upload

- **Method:** POST
- **Path:** `/api/albums/{albumId}/art`
- **Authentication:** Session cookie; any band member (same as art-votes).
- **Notes:** Multipart field `file` (`image/*`, 10 MB cap). The API writes the image into the band Drive folder sandbox and returns it as a vote candidate. Does **not** set `ArtDriveFileId` and does not cast a vote. Archived albums and locked art contests reject uploads. A file that lands outside the folder is rejected. Pixel size is a warning only: if width or height is below 1600 or above 3000, `warning` is set and the upload still succeeds. Drive write/token/scope failures return a JSON error (`409` `upload_failed`, or `403` `needs_reauth` when the member must sign in again for Drive write). Image decode and Drive exceptions are logged and must not take down the API process.

```json
{
  "driveFileId": "cover-tiny",
  "name": "tiny.png",
  "mimeType": "image/png",
  "width": 400,
  "height": 400,
  "warning": "Cover is 400x400. Recommended size is 1600-3000 pixels on each side."
}
```

`warning` is `null` when both sides are in 1600–3000, or when dimensions cannot be read.

## Art preview

- **Method:** GET
- **Path:** `/api/albums/{albumId}/art`
- **Authentication:** Session cookie; any band member.
- **Notes:** With no query, streams the applied cover (`ArtDriveFileId`). `?fileId=` streams that band-folder image for an art-poll thumbnail. Same sandbox as list/play/link: missing file, a file outside the folder, an invalid id, or a non-image returns 404. If the applied cover is gone from Drive (404 / not found / trashed), `GET /api/albums/{id}` and the applied-cover preview clear `ArtDriveFileId` and persist that. Candidate `?fileId=` previews do not clear the applied cover. Does not change audio playback. A Drive outage (`Unknown`) does not clear the cover.

## Drive files for art

`GET /api/bands/{bandId}/drive/files` stays audio-only by default (take picker). `?kind=image` lists image files under the same folder sandbox. `kind` must be `audio` or `image`.
