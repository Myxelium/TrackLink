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
        var song = await db.Songs.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == songId, cancellationToken);
        if (song is null || string.IsNullOrWhiteSpace(song.Url))
        {
            return null;
        }

        if (song.Url.StartsWith(DrivePrefix, StringComparison.OrdinalIgnoreCase))
        {
            var fileId = song.Url[DrivePrefix.Length..];
            var drive = await googleDrive.DownloadAsync(fileId, cancellationToken);
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
