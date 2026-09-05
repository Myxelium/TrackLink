## Get started

TrackLink stores band/song **metadata**. Audio files stay in Google Drive (or any HTTP URL).

1. Start SQL Server: `./start-database.sh` (Docker).
2. API: `dotnet run --project api` (http://localhost:5180).
3. App: `cd app && npm start` (http://localhost:4200). `/api` is proxied to the API.

Lint the Angular client with `cd app && npm run lint` (junolint) or `npm run lint:fix`.

Agent instructions: [`AGENTS.md`](AGENTS.md). Feature contracts: [`agents-docs/FEATURES.md`](agents-docs/FEATURES.md).

## Vision
I want to create a tool that handles plans the music production and album assembly for musicians that work together remotely.

The storage will be handled by Google Drive or any other cloud provider, to let other services take care of the storage part. TrackLink would only store and handle metadata.

### Functionality would eventually include this:
- Voting system for what songs should be in what album.
- Voting system for names.
- Voting systen for album art, and song order.
- Review system for adding songs to album, this will work by selecting timestamps from the song and adding a comment.
- Album management.
- See song versions.
- Search songs.
- Google authorization to access drive storage.

see [Project for progress, and todos](https://github.com/users/Myxelium/projects/1)
