using System.Net.Http.Json;
using System.Text.Json.Serialization;
using api.Contracts;
using api.Data;
using api.Data.Entities;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace api.Integrations.Google;

public class GoogleDriveService(
    DatabaseContext db,
    IOptions<GoogleOptions> options,
    ILogger<GoogleDriveService> logger) : IGoogleDriveService
{
    private static readonly string[] Scopes =
    [
        DriveService.Scope.DriveReadonly,
        "openid",
        "email"
    ];

    public bool IsConfigured
    {
        get
        {
            var o = options.Value;
            return !string.IsNullOrWhiteSpace(o.ClientId) && !string.IsNullOrWhiteSpace(o.ClientSecret);
        }
    }

    public async Task<GoogleStatusDto> GetStatusAsync(CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            return new GoogleStatusDto(false, false, null);
        }

        var account = await db.GoogleAccounts.AsNoTracking().OrderBy(a => a.Id).FirstOrDefaultAsync(cancellationToken);
        return new GoogleStatusDto(true, account is not null, account?.Email);
    }

    public Task<string?> CreateAuthorizationUrlAsync(CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            return Task.FromResult<string?>(null);
        }

        var flow = CreateFlow();
        var request = (GoogleAuthorizationCodeRequestUrl)flow.CreateAuthorizationCodeRequest(options.Value.RedirectUri);
        request.AccessType = "offline";
        return Task.FromResult<string?>(request.Build().AbsoluteUri);
    }

    public async Task<bool> HandleCallbackAsync(string code, CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            return false;
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
            return false;
        }

        if (string.IsNullOrWhiteSpace(token.RefreshToken) && string.IsNullOrWhiteSpace(token.AccessToken))
        {
            return false;
        }

        var email = await TryReadEmailAsync(token.AccessToken, cancellationToken);
        var existing = await db.GoogleAccounts.OrderBy(a => a.Id).FirstOrDefaultAsync(cancellationToken);
        if (existing is null)
        {
            existing = new GoogleAccount();
            db.GoogleAccounts.Add(existing);
        }

        existing.AccessToken = token.AccessToken ?? existing.AccessToken;
        if (!string.IsNullOrWhiteSpace(token.RefreshToken))
        {
            existing.RefreshToken = token.RefreshToken;
        }

        existing.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresInSeconds ?? 3500);
        existing.Email = email ?? existing.Email;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<DriveFileDto>> ListAudioFilesAsync(CancellationToken cancellationToken)
    {
        var service = await CreateDriveServiceAsync(cancellationToken);
        if (service is null)
        {
            return [];
        }

        var request = service.Files.List();
        request.Q = "mimeType contains 'audio/' and trashed = false";
        request.Fields = "files(id, name, mimeType)";
        request.PageSize = 50;
        var result = await request.ExecuteAsync(cancellationToken);
        return (result.Files ?? [])
            .Select(f => new DriveFileDto(f.Id, f.Name, f.MimeType))
            .ToList();
    }

    public async Task<DriveDownload?> DownloadAsync(string fileId, CancellationToken cancellationToken)
    {
        var service = await CreateDriveServiceAsync(cancellationToken);
        if (service is null)
        {
            return null;
        }

        try
        {
            var meta = await service.Files.Get(fileId).ExecuteAsync(cancellationToken);
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

    private GoogleAuthorizationCodeFlow CreateFlow()
    {
        var o = options.Value;
        return new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = o.ClientId,
                ClientSecret = o.ClientSecret
            },
            Scopes = Scopes,
            Prompt = "consent"
        });
    }

    private async Task<DriveService?> CreateDriveServiceAsync(CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            return null;
        }

        var account = await db.GoogleAccounts.OrderBy(a => a.Id).FirstOrDefaultAsync(cancellationToken);
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

        var credential = new UserCredential(flow, "tracklink", token);
        if (account.ExpiresAt <= DateTimeOffset.UtcNow.AddMinutes(1))
        {
            if (!await credential.RefreshTokenAsync(cancellationToken))
            {
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

    private static async Task<string?> TryReadEmailAsync(string? accessToken, CancellationToken cancellationToken)
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

            var json = await response.Content.ReadFromJsonAsync<UserInfo>(cancellationToken);
            return json?.Email;
        }
        catch
        {
            return null;
        }
    }

    private sealed class UserInfo
    {
        [JsonPropertyName("email")]
        public string? Email { get; set; }
    }
}
