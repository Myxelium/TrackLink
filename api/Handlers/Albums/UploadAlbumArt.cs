using api.Contracts;
using api.Data;
using api.Integrations.Google;
using api.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace api.Handlers.Albums;

public static class UploadAlbumArt
{
    public record Command(
        int AlbumId,
        int MemberId,
        string? FileName,
        string? ContentType,
        Stream Content,
        long ContentLength) : IRequest<AlbumActionResult<AlbumArtUploadDto>>;

    public class Handler(
        DatabaseContext db,
        IGoogleDriveService googleDrive,
        ILogger<Handler> logger)
        : IRequestHandler<Command, AlbumActionResult<AlbumArtUploadDto>>
    {
        public async Task<AlbumActionResult<AlbumArtUploadDto>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            try
            {
                return await UploadAsync(request, cancellationToken);
            }
            catch (DriveWriteDeniedException exception)
            {
                logger.LogWarning(exception, "Album art upload needs a new Google login for album {AlbumId}", request.AlbumId);
                return new AlbumActionResult<AlbumArtUploadDto>("needs_reauth", null);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Album art upload failed for album {AlbumId}", request.AlbumId);
                return new AlbumActionResult<AlbumArtUploadDto>("upload_failed", null);
            }
        }

        private async Task<AlbumActionResult<AlbumArtUploadDto>> UploadAsync(
            Command request,
            CancellationToken cancellationToken)
        {
            var album = await LoadAlbum.ById(db, request.AlbumId, cancellationToken);
            if (album is null)
            {
                return new AlbumActionResult<AlbumArtUploadDto>("not_found", null);
            }

            if (album.Archived)
            {
                return new AlbumActionResult<AlbumArtUploadDto>("archived", null);
            }

            if (album.ArtLocked)
            {
                return new AlbumActionResult<AlbumArtUploadDto>("art_locked", null);
            }

            var members = await LoadAlbum.BandMembers(db, album.BandId, cancellationToken);
            if (members.All(member => member.MemberId != request.MemberId))
            {
                return new AlbumActionResult<AlbumArtUploadDto>("not_in_band", null);
            }

            var folderId = album.Band.DriveFolderId;
            if (string.IsNullOrWhiteSpace(folderId))
            {
                return new AlbumActionResult<AlbumArtUploadDto>("folder_missing", null);
            }

            if (request.ContentLength is <= 0 or > AlbumArtPixels.MaxBytes)
            {
                return new AlbumActionResult<AlbumArtUploadDto>(
                    request.ContentLength > AlbumArtPixels.MaxBytes ? "too_large" : "empty_file",
                    null);
            }

            await using var buffer = new MemoryStream();
            var copyError = await AlbumArtPixels.CopyCappedAsync(
                request.Content,
                buffer,
                AlbumArtPixels.MaxBytes,
                cancellationToken);
            if (copyError is not null)
            {
                return new AlbumActionResult<AlbumArtUploadDto>(copyError, null);
            }

            var imageBytes = buffer.ToArray();
            var sniffedMime = AlbumArtPixels.SniffMime(imageBytes, request.ContentType);
            if (sniffedMime is null || !DriveFileKinds.IsImageMime(sniffedMime))
            {
                return new AlbumActionResult<AlbumArtUploadDto>("not_image", null);
            }

            var mimeType = sniffedMime;
            var (width, height) = AlbumArtPixels.TryRead(imageBytes);
            var warning = AlbumArtPixels.Warn(width, height);
            var fileName = SafeFileName(request.FileName);

            buffer.Position = 0;
            var uploaded = await googleDrive.UploadFileAsync(
                request.MemberId,
                folderId,
                fileName,
                mimeType,
                buffer,
                cancellationToken);
            var driveFileId = AlbumArtVotes.Normalize(uploaded?.Id);
            if (driveFileId is null)
            {
                return new AlbumActionResult<AlbumArtUploadDto>("upload_failed", null);
            }

            if (!await googleDrive.IsFileInsideFolderAsync(
                    request.MemberId,
                    driveFileId,
                    folderId,
                    cancellationToken))
            {
                return new AlbumActionResult<AlbumArtUploadDto>("outside_folder", null);
            }

            return new AlbumActionResult<AlbumArtUploadDto>(
                null,
                new AlbumArtUploadDto(
                    driveFileId,
                    uploaded!.Name,
                    uploaded.MimeType ?? mimeType,
                    width,
                    height,
                    warning));
        }

        private static string SafeFileName(string? fileName)
        {
            var name = Path.GetFileName(fileName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return "cover.png";
            }

            return name.Length > 255 ? name[..255] : name;
        }
    }
}
