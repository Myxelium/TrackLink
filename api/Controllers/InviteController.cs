using api.Contracts;
using api.Handlers.Invites;
using api.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/invites")]
public class InviteController(ISender mediator, IMemberSession memberSession) : ControllerBase
{
    [HttpPost("accept")]
    public async Task<IActionResult> Accept(
        [FromBody] AcceptInviteRequest body,
        CancellationToken cancellationToken)
    {
        var email = body.Email?.Trim() ?? "";
        var code = body.Code?.Trim() ?? "";
        if (email.Length == 0 || code.Length == 0)
        {
            return BadRequest(new { error = "email and code are required" });
        }

        var result = await mediator.Send(
            new AcceptBandInvite.Command(email, code, memberSession.GetMemberId(HttpContext)),
            cancellationToken);

        if (result.Accepted)
        {
            return Ok(result);
        }

        if (result.NeedsLogin)
        {
            return Unauthorized(result);
        }

        return BadRequest(result);
    }
}
