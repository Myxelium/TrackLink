# Song

Studio playback binds `<audio>` to `/api/songs/{id}/audio` only. Never use the raw storage pointer in the client.

A signed-in member lists audio under the band Drive folder. Linking one POSTs `/api/bands/{id}/songs` with `driveFileId` so the catalog stores `gdrive:{fileId}` and play stays on the song-id endpoint using that member's Google token.

Takes and the propose picker omit Drive-backed songs whose file sits outside the band folder. The SQL row stays.

Linking a Drive file with the same name as an existing take creates the next version. The same Drive file id returns the existing row. Takes show `2 ← 1` and the Drive modified day when present.
