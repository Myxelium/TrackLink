# Google Drive

> **Area:** google-drive
> **Status:** In Progress
> **Last updated:** 2026-09-04

## Overview

Google Drive holds audio bytes. TrackLink stores only a file pointer on `Song.Url` (`gdrive:{fileId}`). OAuth is a web flow against Google; tokens persist in SQL Server.

## Responsibilities

- OAuth login/callback and token refresh
- List audio files the user can access
- Download a Drive file for the play endpoint
- Does **not** own song metadata

## Key concepts

- **Drive pointer**: `gdrive:{fileId}` on `Song.Url`
- **Google account**: refresh + access token row used by the API to call Drive

## API Endpoint

### Status

- **Method:** GET
- **Path:** `/api/auth/google/status`
- Returns `{ "configured": true|false, "connected": true|false, "email": "..." | null }`
- `configured` is false when `Google__ClientId` / `Google__ClientSecret` are missing

### Login

- **Method:** GET
- **Path:** `/api/auth/google/login`
- Redirects to Google. Returns 503 if not configured.

### Callback

- **Method:** GET
- **Path:** `/api/auth/google/callback`
- Exchanges `code`, stores tokens, redirects to the Angular app (`/`).

### Files

- **Method:** GET
- **Path:** `/api/drive/files`
- Lists audio MIME types the connected account can read.

## Config

| Key | Env |
|-----|-----|
| `Google:ClientId` | `Google__ClientId` |
| `Google:ClientSecret` | `Google__ClientSecret` |
| `Google:RedirectUri` | default `http://localhost:5180/api/auth/google/callback` |

OAuth scopes: `https://www.googleapis.com/auth/drive.readonly`, `openid`, `email`.
