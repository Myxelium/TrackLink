using api.Contracts;

namespace api.Integrations.Google;

public interface IGoogleDriveService
{
    bool IsConfigured { get; }

    Task<GoogleStatusDto> GetStatusAsync(CancellationToken cancellationToken);

    Task<string?> CreateAuthorizationUrlAsync(CancellationToken cancellationToken);

    Task<bool> HandleCallbackAsync(string code, CancellationToken cancellationToken);

    Task<IReadOnlyList<DriveFileDto>> ListAudioFilesAsync(CancellationToken cancellationToken);

    Task<DriveDownload?> DownloadAsync(string fileId, CancellationToken cancellationToken);
}

public sealed class DriveDownload
{
    public required Stream Stream { get; init; }
    public required string ContentType { get; init; }
}
