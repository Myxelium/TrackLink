using System.Net.Http.Json;
using System.Text.Json.Serialization;
using api.Contracts;
using api.Data;
using api.Data.Entities;
using api.Services;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace api.Integrations.Google;

public class GoogleDriveService(
    DatabaseContext db,
    IOptions<GoogleOptions> options,
    ILogger<GoogleDriveService> logger) : IGoogleDriveService
{
    private const string FolderMime = "application/vnd.google-apps.folder";

    private static readonly string[] Scopes =
    [
        DriveService.Scope.Drive,
        "openid",
        "email",
        "profile"
    ];

    public bool IsConfigured
    {
        get
        {
            var configured = options.Value;
            return !string.IsNullOrWhiteSpace(configured.ClientId) &&
                   !string.IsNullOrWhiteSpace(configured.ClientSecret);
        }
    }

    public Task<string?> CreateAuthorizationUrlAsync(string? state, CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            return Task.FromResult<string?>(null);
        }

        var flow = CreateFlow();
        var request = (GoogleAuthorizationCodeRequestUrl)flow.CreateAuthorizationCodeRequest(options.Value.RedirectUri);
        request.AccessType = "offline";
        if (!string.IsNullOrWhiteSpace(state))
        {
            request.State = state;
        }

        return Task.FromResult<string?>(request.Build().AbsoluteUri);
    }

    public async Task<GoogleLoginProfile?> ExchangeCodeAsync(string code, CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            return null;
        }

        var flow = CreateFlow();
        TokenResponse token;
        try
        {
            token = await flow.ExchangeCodeForTokenAsync("tracklink", code, options.Value.RedirectUri, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Google OAuth code exchange failed");
            return null;
        }

        if (string.IsNullOrWhiteSpace(token.AccessToken))
        {
            return null;
        }

        var info = await TryReadUserInfoAsync(token.AccessToken, cancellationToken);
        return new GoogleLoginProfile
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresInSeconds ?? 3500),
            Email = info?.Email,
            Subject = info?.Id,
            Name = info?.Name,
            Picture = info?.Picture
        };
    }

    public async Task SaveTokensAsync(int memberId, GoogleLoginProfile profile, CancellationToken cancellationToken)
    {
        var account = await db.GoogleAccounts.FirstOrDefaultAsync(
            googleAccount => googleAccount.MemberId == memberId,
            cancellationToken);
        if (account is null)
        {
            account = new GoogleAccount { MemberId = memberId };
            db.GoogleAccounts.Add(account);
        }

        account.AccessToken = profile.AccessToken;
        if (!string.IsNullOrWhiteSpace(profile.RefreshToken))
        {
            account.RefreshToken = profile.RefreshToken;
        }
        else if (string.IsNullOrWhiteSpace(account.RefreshToken))
        {
            account.RefreshToken = profile.AccessToken;
        }

        account.ExpiresAt = profile.ExpiresAt;
        account.Email = profile.Email ?? account.Email;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasTokensAsync(int memberId, CancellationToken cancellationToken)
    {
        return await db.GoogleAccounts.AsNoTracking().AnyAsync(
            googleAccount => googleAccount.MemberId == memberId && googleAccount.RefreshToken != "",
            cancellationToken);
    }

    public async Task<string?> GetEmailAsync(int memberId, CancellationToken cancellationToken)
    {
        return await db.GoogleAccounts.AsNoTracking()
            .Where(googleAccount => googleAccount.MemberId == memberId)
            .Select(googleAccount => googleAccount.Email)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<string?> GetAccessTokenAsync(int memberId, CancellationToken cancellationToken)
    {
        var service = await CreateDriveServiceAsync(memberId, cancellationToken);
        if (service is null)
        {
            return null;
        }

        var account = await db.GoogleAccounts.AsNoTracking()
            .FirstOrDefaultAsync(googleAccount => googleAccount.MemberId == memberId, cancellationToken);
        return account?.AccessToken;
    }

    public Task<IReadOnlyList<DriveFileDto>> ListAudioFilesAsync(
        int memberId,
        string folderId,
        CancellationToken cancellationToken)
    {
        return ListFolderFilesAsync(memberId, folderId, "audio/", "audio", cancellationToken);
    }

    public Task<IReadOnlyList<DriveFileDto>> ListImageFilesAsync(
        int memberId,
        string folderId,
        CancellationToken cancellationToken)
    {
        return ListFolderFilesAsync(memberId, folderId, "image/", "image", cancellationToken);
    }

    private async Task<IReadOnlyList<DriveFileDto>> ListFolderFilesAsync(
        int memberId,
        string folderId,
        string mimePrefix,
        string fallbackName,
        CancellationToken cancellationToken)
    {
        var service = await CreateDriveServiceAsync(memberId, cancellationToken);
        if (service is null)
        {
            return [];
        }

        var listed = new List<DriveFileDto>();
        var folders = new Queue<string>();
        var seenFolders = new HashSet<string>(StringComparer.Ordinal) { folderId };
        folders.Enqueue(folderId);

        while (folders.Count > 0 && listed.Count < 200)
        {
            var parentId = folders.Dequeue();
                IList<global::Google.Apis.Drive.v3.Data.File> files;
            try
            {
                var request = service.Files.List();
                request.Q = $"'{EscapeQueryValue(parentId)}' in parents and trashed = false";
                request.Fields = "files(id, name, mimeType)";
                request.PageSize = 100;
                request.SupportsAllDrives = true;
                request.IncludeItemsFromAllDrives = true;
                var result = await request.ExecuteAsync(cancellationToken);
                files = result.Files ?? [];
            }
            catch (Exception ex)
            {
                logger.LogInformation(ex, "Drive list denied or failed under {FolderId}", parentId);
                if (parentId == folderId)
                {
                    throw new DriveFolderDeniedException();
                }

                continue;
            }

            foreach (var file in files)
            {
                if (string.IsNullOrWhiteSpace(file.Id))
                {
                    continue;
                }

                if (string.Equals(file.MimeType, FolderMime, StringComparison.OrdinalIgnoreCase))
                {
                    if (seenFolders.Add(file.Id))
                    {
                        folders.Enqueue(file.Id);
                    }

                    continue;
                }

                if (file.MimeType is not null &&
                    file.MimeType.StartsWith(mimePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    listed.Add(new DriveFileDto(file.Id, file.Name ?? fallbackName, file.MimeType));
                }
            }
        }

        return listed;
    }

    public async Task<bool> CanReadFolderAsync(int memberId, string folderId, CancellationToken cancellationToken)
    {
        var service = await CreateDriveServiceAsync(memberId, cancellationToken);
        if (service is null)
        {
            return false;
        }

        try
        {
            var request = service.Files.Get(folderId);
            request.Fields = "id, mimeType";
            request.SupportsAllDrives = true;
            var file = await request.ExecuteAsync(cancellationToken);
            return file is not null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsFileInsideFolderAsync(
        int memberId,
        string fileId,
        string folderId,
        CancellationToken cancellationToken)
    {
        var service = await CreateDriveServiceAsync(memberId, cancellationToken);
        if (service is null)
        {
            return false;
        }

        var parentsById = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        var current = fileId;
        for (var depth = 0; depth < 32; depth++)
        {
            if (string.Equals(current, folderId, StringComparison.Ordinal))
            {
                return true;
            }

            global::Google.Apis.Drive.v3.Data.File meta;
            try
            {
                var request = service.Files.Get(current);
                request.Fields = "id, parents";
                request.SupportsAllDrives = true;
                meta = await request.ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogInformation(ex, "Drive parent walk failed for {FileId}", current);
                return false;
            }

            var parents = (meta.Parents ?? []).ToList();
            parentsById[current] = parents;
            if (DriveFolderScope.IsInsideRoot(fileId, folderId, parentsById))
            {
                return true;
            }

            if (parents.Count == 0)
            {
                return false;
            }

            current = parents[0];
        }

        return false;
    }

    public async Task<DriveFileDto?> GetAudioFileAsync(
        int memberId,
        string fileId,
        CancellationToken cancellationToken)
    {
        var service = await CreateDriveServiceAsync(memberId, cancellationToken);
        if (service is null)
        {
            return null;
        }

        try
        {
            var request = service.Files.Get(fileId);
            request.Fields = "id, name, mimeType, md5Checksum, modifiedTime";
            request.SupportsAllDrives = true;
            var file = await request.ExecuteAsync(cancellationToken);
            if (file is null || string.IsNullOrWhiteSpace(file.Id))
            {
                return null;
            }

            return new DriveFileDto(
                file.Id,
                file.Name ?? "audio",
                file.MimeType,
                string.IsNullOrWhiteSpace(file.Md5Checksum) ? null : file.Md5Checksum,
                file.ModifiedTimeDateTimeOffset?.UtcDateTime);
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Drive file metadata failed for {FileId}", fileId);
            return null;
        }
    }

    public async Task<DriveDownload?> DownloadAsync(int memberId, string fileId, CancellationToken cancellationToken)
    {
        var service = await CreateDriveServiceAsync(memberId, cancellationToken);
        if (service is null)
        {
            return null;
        }

        try
        {
            var metaRequest = service.Files.Get(fileId);
            metaRequest.SupportsAllDrives = true;
            var meta = await metaRequest.ExecuteAsync(cancellationToken);
            var stream = new MemoryStream();
            await service.Files.Get(fileId).DownloadAsync(stream, cancellationToken);
            stream.Position = 0;
            return new DriveDownload
            {
                Stream = stream,
                ContentType = string.IsNullOrWhiteSpace(meta.MimeType) ? "audio/mpeg" : meta.MimeType
            };
        }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Drive download failed for {FileId}", fileId);
                return null;
            }
        }

    public async Task<DriveFileDto?> UploadFileAsync(
        int memberId,
        string folderId,
        string fileName,
        string mimeType,
        Stream content,
        CancellationToken cancellationToken)
    {
        DriveService? service;
        try
        {
            service = await CreateDriveServiceAsync(memberId, cancellationToken);
        }
        catch (Exception ex) when (DriveAuthErrors.IsDriveAuthFailure(ex))
        {
            logger.LogWarning(ex, "Drive client create needs a new Google login for upload under {FolderId}", folderId);
            throw new DriveWriteDeniedException();
        }

        if (service is null)
        {
            if (IsConfigured && await HasTokensAsync(memberId, cancellationToken))
            {
                throw new DriveWriteDeniedException();
            }

            return null;
        }

        var metadata = new global::Google.Apis.Drive.v3.Data.File
        {
            Name = fileName,
            Parents = [folderId]
        };

        try
        {
            var request = service.Files.Create(metadata, content, mimeType);
            request.Fields = "id, name, mimeType";
            request.SupportsAllDrives = true;
            var progress = await request.UploadAsync(cancellationToken);
            if (progress.Exception is not null && DriveAuthErrors.IsDriveAuthFailure(progress.Exception))
            {
                throw new DriveWriteDeniedException();
            }

            if (progress.Status != UploadStatus.Completed || request.ResponseBody is null)
            {
                logger.LogWarning(
                    "Drive image upload did not complete under {FolderId}: {Status}",
                    folderId,
                    progress.Status);
                return null;
            }

            var file = request.ResponseBody;
            if (string.IsNullOrWhiteSpace(file.Id))
            {
                return null;
            }

            return new DriveFileDto(file.Id, file.Name ?? fileName, file.MimeType ?? mimeType);
        }
        catch (DriveWriteDeniedException)
        {
            throw;
        }
        catch (Exception ex) when (DriveAuthErrors.IsDriveAuthFailure(ex))
        {
            logger.LogWarning(ex, "Drive image upload needs a new Google login under {FolderId}", folderId);
            throw new DriveWriteDeniedException();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Drive image upload failed under {FolderId}", folderId);
            return null;
        }
    }

    public async Task<DriveFilePresence> GetFilePresenceAsync(
        int memberId,
        string fileId,
        CancellationToken cancellationToken)
    {
        var service = await CreateDriveServiceAsync(memberId, cancellationToken);
        if (service is null)
        {
            return DriveFilePresence.Unknown;
        }

        try
        {
            var request = service.Files.Get(fileId);
            request.Fields = "id, trashed";
            request.SupportsAllDrives = true;
            var file = await request.ExecuteAsync(cancellationToken);
            if (file is null || string.IsNullOrWhiteSpace(file.Id) || file.Trashed == true)
            {
                return DriveFilePresence.NotFound;
            }

            return DriveFilePresence.Found;
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return DriveFilePresence.NotFound;
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Drive file presence check failed for {FileId}", fileId);
            return DriveFilePresence.Unknown;
        }
    }

    private GoogleAuthorizationCodeFlow CreateFlow()
    {
        var configured = options.Value;
        return new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = configured.ClientId,
                ClientSecret = configured.ClientSecret
            },
            Scopes = Scopes,
            Prompt = "consent"
        });
    }

    private async Task<DriveService?> CreateDriveServiceAsync(int memberId, CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            return null;
        }

        var account = await db.GoogleAccounts.FirstOrDefaultAsync(
            googleAccount => googleAccount.MemberId == memberId,
            cancellationToken);
        if (account is null || string.IsNullOrWhiteSpace(account.RefreshToken))
        {
            return null;
        }

        var flow = CreateFlow();
        var token = new TokenResponse
        {
            AccessToken = account.AccessToken,
            RefreshToken = account.RefreshToken,
            ExpiresInSeconds = (long)Math.Max(0, (account.ExpiresAt - DateTimeOffset.UtcNow).TotalSeconds)
        };

        var credential = new UserCredential(flow, $"member:{memberId}", token);
        if (account.ExpiresAt <= DateTimeOffset.UtcNow.AddMinutes(1))
        {
            try
            {
                if (!await credential.RefreshTokenAsync(cancellationToken))
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Drive token refresh failed for member {MemberId}", memberId);
                return null;
            }

            account.AccessToken = credential.Token.AccessToken;
            if (!string.IsNullOrWhiteSpace(credential.Token.RefreshToken))
            {
                account.RefreshToken = credential.Token.RefreshToken;
            }

            account.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(credential.Token.ExpiresInSeconds ?? 3500);
            await db.SaveChangesAsync(cancellationToken);
        }

        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "TrackLink"
        });
    }

    private static async Task<UserInfo?> TryReadUserInfoAsync(string? accessToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return null;
        }

        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await http.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<UserInfo>(cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private static string EscapeQueryValue(string value)
    {
        return value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("'", "\\'", StringComparison.Ordinal);
    }

    private sealed class UserInfo
    {
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("picture")]
        public string? Picture { get; set; }
    }
}

public sealed class DriveFolderDeniedException : Exception;

public sealed class DriveWriteDeniedException : Exception;

public static class DriveAuthErrors
{
    public static bool IsDriveAuthFailure(Exception exception)
    {
        if (exception is DriveWriteDeniedException or TokenResponseException)
        {
            return true;
        }

        if (exception is GoogleApiException googleException)
        {
            if (googleException.HttpStatusCode is
                System.Net.HttpStatusCode.Unauthorized or
                System.Net.HttpStatusCode.Forbidden)
            {
                return true;
            }
        }

        var text = exception.Message ?? string.Empty;
        return text.Contains("insufficient", StringComparison.OrdinalIgnoreCase) ||
               text.Contains("invalid_grant", StringComparison.OrdinalIgnoreCase) ||
               text.Contains("ACCESS_TOKEN_SCOPE", StringComparison.OrdinalIgnoreCase) ||
               text.Contains("unauthenticated", StringComparison.OrdinalIgnoreCase);
    }
}
