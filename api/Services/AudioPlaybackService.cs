using api.Data;
using api.Integrations.Google;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class AudioPlaybackService(
    DatabaseContext db,
    IHttpClientFactory httpClientFactory,
    IGoogleDriveService googleDrive) : IAudioPlaybackService
{
    public const string DrivePrefix = "gdrive:";

    public async Task<AudioOpenResult?> OpenAsync(int songId, CancellationToken cancellationToken)
    {
        return await OpenAsync(songId, null, cancellationToken);
    }

    public async Task<AudioOpenResult?> OpenAsync(int songId, int? memberId, CancellationToken cancellationToken)
    {
        var song = await db.Songs.AsNoTracking()
            .FirstOrDefaultAsync(catalogSong => catalogSong.Id == songId, cancellationToken);
        if (song is null || string.IsNullOrWhiteSpace(song.Url))
        {
            return null;
        }

        if (song.Url.StartsWith(DrivePrefix, StringComparison.OrdinalIgnoreCase))
        {
            if (memberId is null)
            {
                return null;
            }

            var fileId = song.Url[DrivePrefix.Length..];
            var band = await db.SongIdentifiers.AsNoTracking()
                .Where(identifier => identifier.SongId == songId)
                .Select(identifier => identifier.Band)
                .FirstOrDefaultAsync(cancellationToken);
            if (band?.DriveFolderId is null)
            {
                return null;
            }

            var inBand = await db.BandMembers.AsNoTracking().AnyAsync(
                bandMember => bandMember.BandId == band.Id && bandMember.MemberId == memberId,
                cancellationToken);
            if (!inBand)
            {
                return null;
            }

            if (!await googleDrive.IsFileInsideFolderAsync(
                    memberId.Value,
                    fileId,
                    band.DriveFolderId,
                    cancellationToken))
            {
                return null;
            }

            var drive = await googleDrive.DownloadAsync(memberId.Value, fileId, cancellationToken);
            if (drive is null)
            {
                return null;
            }

            return new AudioOpenResult
            {
                Stream = drive.Stream,
                ContentType = drive.ContentType,
                EnableRangeProcessing = drive.Stream.CanSeek
            };
        }

        if (!Uri.TryCreate(song.Url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return null;
        }

        var client = httpClientFactory.CreateClient("audio");
        var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "audio/mpeg";
        var networkStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var buffer = new MemoryStream();
        await networkStream.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;

        return new AudioOpenResult
        {
            Stream = buffer,
            ContentType = contentType,
            EnableRangeProcessing = true
        };
    }
}
