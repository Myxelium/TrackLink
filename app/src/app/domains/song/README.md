# Song

Studio playback binds `<audio>` to `/api/songs/{id}/audio` only. Never use the raw storage pointer in the client.

A signed-in member lists audio under the band Drive folder. Linking one POSTs `/api/bands/{id}/songs` with `driveFileId` so the catalog stores `gdrive:{fileId}` and play stays on the song-id endpoint using that member's Google token.
