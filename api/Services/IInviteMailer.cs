namespace api.Services;

public interface IInviteMailer
{
    Task<bool> SendInviteAsync(
        string toEmail,
        string bandName,
        string acceptUrl,
        string code,
        CancellationToken cancellationToken);
}
