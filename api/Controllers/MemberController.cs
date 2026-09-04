using api.Handlers.Members;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/members")]
public class MemberController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var members = await mediator.Send(new ListMembers.Query(), cancellationToken);
        return Ok(members);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var member = await mediator.Send(new GetMember.Query(id), cancellationToken);
        return member is null ? NotFound() : Ok(member);
    }
}
