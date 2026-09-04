using api.Integrations.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace api.Controllers;

[ApiController]
public class GoogleAuthController(
    IGoogleDriveService googleDrive,
    IOptions<GoogleOptions> options) : ControllerBase
{
    [HttpGet("api/auth/google/status")]
    public async Task<IActionResult> Status(CancellationToken cancellationToken)
    {
        return Ok(await googleDrive.GetStatusAsync(cancellationToken));
    }

    [HttpGet("api/auth/google/login")]
    public async Task<IActionResult> Login(CancellationToken cancellationToken)
    {
        var url = await googleDrive.CreateAuthorizationUrlAsync(cancellationToken);
        if (url is null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                error = "Google OAuth is not configured. Set Google__ClientId and Google__ClientSecret."
            });
        }

        return Redirect(url);
    }

    [HttpGet("api/auth/google/callback")]
    public async Task<IActionResult> Callback([FromQuery] string? code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(new { error = "Missing code" });
        }

        var ok = await googleDrive.HandleCallbackAsync(code, cancellationToken);
        if (!ok)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "Google OAuth exchange failed" });
        }

        return Redirect(options.Value.AppReturnUrl);
    }

    [HttpGet("api/drive/files")]
    public async Task<IActionResult> Files(CancellationToken cancellationToken)
    {
        var status = await googleDrive.GetStatusAsync(cancellationToken);
        if (!status.Configured)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "Google OAuth is not configured" });
        }

        if (!status.Connected)
        {
            return StatusCode(StatusCodes.Status401Unauthorized, new { error = "Connect Google Drive first" });
        }

        return Ok(await googleDrive.ListAudioFilesAsync(cancellationToken));
    }
}
