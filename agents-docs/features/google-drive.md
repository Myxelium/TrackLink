# Google Drive

> **Area:** google-drive
> **Status:** Active
> **Last updated:** 2026-09-04

## Overview

Each TrackLink member signs in with Google. That login creates or updates the Member row and stores that person's Drive tokens. A band has one Drive folder root. List, link, and play never walk above it. Tokens from another member are never used.

## Responsibilities

- Google login, session cookie, status, logout, and Picker token
- Folder-scoped Drive listing and download
- Does **not** store audio in SQL Server

## Key concepts

- **Drive pointer**: `gdrive:{fileId}` on `Song.Url`
- **Member tokens**: one `GoogleAccount` row per member
- **Band root**: `Band.DriveFolderId` set by the owner via Google Picker

## API Endpoint

### Session

- **Method:** GET
- **Path:** `/api/auth/me` (also `/api/auth/google/status`)
- **Authentication:** Cookie `tracklink.sid` when signed in

```json
{
  "configured": true,
  "signedIn": true,
  "connected": true,
  "email": "ada@example.com",
  "member": { "id": 4, "username": "ada", "email": "ada@example.com", "bands": [] }
}
```

### Login / callback

- **Method:** GET
- **Path:** `/api/auth/google/login` then `/api/auth/google/callback`
- **Notes:** Redirect URI is `http://localhost:5180/api/auth/google/callback`. Query `?invite={code}` is passed as OAuth state. After consent the API sets the session cookie and redirects to `AppReturnUrl` (or `/join?code=` when an invite is pending).
- First login upserts `Member` from Google profile (email, name, picture) and stores refresh/access tokens on that member.

### Picker token

- **Method:** GET
- **Path:** `/api/auth/google/picker-token`
- **Authentication:** Session cookie

```json
{ "accessToken": "...", "clientId": "...", "apiKey": "optional-browser-key" }
```

`Google:ApiKey` is the browser developer key for Google Picker. Folder pick uses Drive as the source and folders only.

### List audio in the band folder

- **Method:** GET
- **Path:** `/api/bands/{bandId}/drive/files`
- **Authentication:** Session cookie; member must belong to the band

Files outside the band root are not returned. A file the member cannot read is omitted. If Drive denies the folder itself: 403.

### Set band folder

- **Method:** PUT
- **Path:** `/api/bands/{bandId}/drive-folder`
- **Authentication:** Session cookie; owner only

```json
{ "folderId": "1abc", "name": "Kindred takes" }
```
