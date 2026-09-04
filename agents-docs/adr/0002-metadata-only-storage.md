# ADR-0002: Metadata in SQL Server, bytes in cloud storage

## Status
Accepted

## Context
Remote bands already keep masters in Google Drive (or another cloud). Duplicating audio blobs in TrackLink would fight that workflow and bloat SQL Server.

## Decision
SQL Server stores only metadata and a storage pointer (`Song.Url`: https URL or `gdrive:{fileId}`). Playback resolves the pointer at request time via `GET /api/songs/{id}/audio`.

## Rationale
Matches the product vision in the root README. Google OAuth/Drive can be enabled later without changing the song table shape.
