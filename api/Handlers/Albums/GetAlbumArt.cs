using api.Data;
using api.Integrations.Google;
using api.Services;
using MediatR;

namespace api.Handlers.Albums;

public static class GetAlbumArt
{
    public record Query(int AlbumId, int MemberId, string? DriveFileId = null) : IRequest<AlbumArtOpenResult>;

    public class Handler(DatabaseContext db, IGoogleDriveService googleDrive)
        : IRequestHandler<Query, AlbumArtOpenResult>
    {
        public async Task<AlbumArtOpenResult> Handle(Query request, CancellationToken cancellationToken)
        {
            var album = await LoadAlbum.ById(db, request.AlbumId, cancellationToken);
            if (album is null)
            {
                return AlbumArtOpenResult.Fail("not_found");
            }

            var members = await LoadAlbum.BandMembers(db, album.BandId, cancellationToken);
            if (members.All(member => member.MemberId != request.MemberId))
            {
                return AlbumArtOpenResult.Fail("not_in_band");
            }

            var requestedCandidate = !string.IsNullOrWhiteSpace(request.DriveFileId);
            if (!requestedCandidate &&
                await AlbumArtStorage.ClearIfMissingAsync(
                    album,
                    googleDrive,
                    request.MemberId,
                    cancellationToken))
            {
                await db.SaveChangesAsync(cancellationToken);
                return AlbumArtOpenResult.Fail("art_missing");
            }

            var fileId = ResolveFileId(request.DriveFileId, album.ArtDriveFileId);
            if (fileId is null && !string.IsNullOrWhiteSpace(request.DriveFileId))
            {
                return AlbumArtOpenResult.Fail("invalid_art");
            }

            if (string.IsNullOrWhiteSpace(fileId))
            {
                return AlbumArtOpenResult.Fail("art_missing");
            }

            var folderId = album.Band.DriveFolderId;
            if (string.IsNullOrWhiteSpace(folderId))
            {
                return AlbumArtOpenResult.Fail("folder_missing");
            }

            if (!await googleDrive.IsFileInsideFolderAsync(
                    request.MemberId,
                    fileId,
                    folderId,
                    cancellationToken))
            {
                return AlbumArtOpenResult.Fail("outside_folder");
            }

            var meta = await googleDrive.GetAudioFileAsync(request.MemberId, fileId, cancellationToken);
            if (!DriveFileKinds.IsImageMime(meta?.MimeType))
            {
                return AlbumArtOpenResult.Fail("not_image");
            }

            var download = await googleDrive.DownloadAsync(request.MemberId, fileId, cancellationToken);
            if (download is null)
            {
                return AlbumArtOpenResult.Fail("not_found");
            }

            return new AlbumArtOpenResult(null, download.Stream, download.ContentType);
        }

        private static string? ResolveFileId(string? requestedFileId, string? appliedFileId)
        {
            if (string.IsNullOrWhiteSpace(requestedFileId))
            {
                return appliedFileId;
            }

            return AlbumArtVotes.Normalize(requestedFileId);
        }
    }
}

public record AlbumArtOpenResult(string? Error, Stream? Stream, string? ContentType)
{
    public static AlbumArtOpenResult Fail(string error) => new(error, null, null);
}
