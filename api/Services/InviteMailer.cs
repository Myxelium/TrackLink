using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace api.Services;

public class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = "";

    public int Port { get; set; } = 587;

    public string? User { get; set; }

    public string? Password { get; set; }

    public string From { get; set; } = "tracklink@localhost";
}

public class InviteMailer(
    IOptions<SmtpOptions> options,
    ILogger<InviteMailer> logger) : IInviteMailer
{
    public async Task<bool> SendInviteAsync(
        string toEmail,
        string bandName,
        string acceptUrl,
        string code,
        CancellationToken cancellationToken)
    {
        var smtp = options.Value;
        if (string.IsNullOrWhiteSpace(smtp.Host))
        {
            logger.LogInformation(
                "Invite for {Email} to {Band} code {Code} url {Url} (SMTP not configured)",
                toEmail,
                bandName,
                code,
                acceptUrl);
            return false;
        }

        try
        {
            using var client = new SmtpClient(smtp.Host, smtp.Port)
            {
                EnableSsl = smtp.Port != 25
            };
            if (!string.IsNullOrWhiteSpace(smtp.User))
            {
                client.Credentials = new NetworkCredential(smtp.User, smtp.Password);
            }

            using var message = new MailMessage(smtp.From, toEmail)
            {
                Subject = $"TrackLink invite: {bandName}",
                Body = $"You were invited to {bandName} on TrackLink.\n\nOpen {acceptUrl}\nor enter this code: {code}\n"
            };
            await client.SendMailAsync(message, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Invite email to {Email} failed", toEmail);
            return false;
        }
    }
}
