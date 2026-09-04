using api.Contracts;
using api.Handlers.Auth;
using api.Integrations.Google;
using api.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace api.Controllers;

[ApiController]
public class GoogleAuthController(
    ISender mediator,
    IGoogleDriveService googleDrive,
    IMemberSession memberSession,
    IOptions<GoogleOptions> options) : ControllerBase
{
    [HttpGet("api/auth/me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        return Ok(await mediator.Send(new GetSession.Query(memberId), cancellationToken));
    }

    [HttpGet("api/auth/google/status")]
    public async Task<IActionResult> Status(CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        return Ok(await mediator.Send(new GetSession.Query(memberId), cancellationToken));
    }

    [HttpGet("api/auth/google/login")]
    public async Task<IActionResult> Login([FromQuery] string? invite, CancellationToken cancellationToken)
    {
        var url = await googleDrive.CreateAuthorizationUrlAsync(invite, cancellationToken);
        if (url is null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                error = "Google OAuth is not configured. Set Google:ClientId and Google:ClientSecret."
            });
        }

        return Redirect(url);
    }

    [HttpGet("api/auth/google/callback")]
    public async Task<IActionResult> Callback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(new { error = "Missing code" });
        }

        var memberId = await mediator.Send(new CompleteGoogleLogin.Command(code, state), cancellationToken);
        if (memberId is null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "Google OAuth exchange failed" });
        }

        memberSession.SignIn(HttpContext, memberId.Value);
        var returnUrl = options.Value.AppReturnUrl;
        if (!string.IsNullOrWhiteSpace(state))
        {
            returnUrl = returnUrl.TrimEnd('/') + "/join?code=" + Uri.EscapeDataString(state);
        }

        return Redirect(returnUrl);
    }

    [HttpPost("api/auth/logout")]
    public IActionResult Logout()
    {
        memberSession.SignOut(HttpContext);
        return Ok(new { signedIn = false });
    }

    [HttpGet("api/auth/google/picker-token")]
    public async Task<IActionResult> PickerToken(CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        if (memberId is null)
        {
            return Unauthorized(new { error = "Sign in with Google first" });
        }

        var accessToken = await googleDrive.GetAccessTokenAsync(memberId.Value, cancellationToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Unauthorized(new { error = "Connect Google Drive first" });
        }

        var configured = options.Value;
        return Ok(new PickerTokenDto(
            accessToken,
            configured.ClientId,
            string.IsNullOrWhiteSpace(configured.ApiKey) ? null : configured.ApiKey));
    }
}
