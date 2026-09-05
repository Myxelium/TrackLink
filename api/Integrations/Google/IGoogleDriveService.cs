using api.Contracts;

namespace api.Integrations.Google;

public interface IGoogleDriveService
{
    bool IsConfigured { get; }

    Task<string?> CreateAuthorizationUrlAsync(string? state, CancellationToken cancellationToken);

    Task<GoogleLoginProfile?> ExchangeCodeAsync(string code, CancellationToken cancellationToken);

    Task SaveTokensAsync(int memberId, GoogleLoginProfile profile, CancellationToken cancellationToken);

    Task<bool> HasTokensAsync(int memberId, CancellationToken cancellationToken);

    Task<string?> GetEmailAsync(int memberId, CancellationToken cancellationToken);

    Task<string?> GetAccessTokenAsync(int memberId, CancellationToken cancellationToken);

    Task<IReadOnlyList<DriveFileDto>> ListAudioFilesAsync(
        int memberId,
        string folderId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DriveFileDto>> ListImageFilesAsync(
        int memberId,
        string folderId,
        CancellationToken cancellationToken);

    Task<bool> CanReadFolderAsync(int memberId, string folderId, CancellationToken cancellationToken);

    Task<bool> IsFileInsideFolderAsync(
        int memberId,
        string fileId,
        string folderId,
        CancellationToken cancellationToken);

    Task<DriveFileDto?> GetAudioFileAsync(
        int memberId,
        string fileId,
        CancellationToken cancellationToken);

    Task<DriveDownload?> DownloadAsync(int memberId, string fileId, CancellationToken cancellationToken);

    Task<DriveFileDto?> UploadFileAsync(
        int memberId,
        string folderId,
        string fileName,
        string mimeType,
        Stream content,
        CancellationToken cancellationToken) =>
        Task.FromResult<DriveFileDto?>(null);

    Task<DriveFilePresence> GetFilePresenceAsync(
        int memberId,
        string fileId,
        CancellationToken cancellationToken) =>
        Task.FromResult(DriveFilePresence.Unknown);
}

public enum DriveFilePresence
{
    Found,
    NotFound,
    Unknown
}

public sealed class GoogleLoginProfile
{
    public required string AccessToken { get; init; }

    public string? RefreshToken { get; init; }

    public DateTimeOffset ExpiresAt { get; init; }

    public string? Email { get; init; }

    public string? Subject { get; init; }

    public string? Name { get; init; }

    public string? Picture { get; init; }
}

public sealed class DriveDownload
{
    public required Stream Stream { get; init; }
    public required string ContentType { get; init; }
}
