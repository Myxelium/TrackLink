using api.Contracts;
using api.Handlers.Albums;
using api.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/albums")]
public class AlbumController(ISender mediator, IMemberSession memberSession) : ControllerBase
{
    [HttpGet("{albumId:int}")]
    public async Task<IActionResult> Get(int albumId, CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        if (memberId is null)
        {
            return Unauthorized(new { error = "Sign in with Google first" });
        }

        var result = await mediator.Send(new GetAlbum.Query(albumId, memberId.Value), cancellationToken);
        return AlbumHttp.From(result);
    }

    [HttpPatch("{albumId:int}")]
    public async Task<IActionResult> Update(
        int albumId,
        [FromBody] UpdateAlbumRequest body,
        CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        if (memberId is null)
        {
            return Unauthorized(new { error = "Sign in with Google first" });
        }

        var result = await mediator.Send(
            new UpdateAlbum.Command(albumId, memberId.Value, body.Name, body.Archived, body.ApprovalRule),
            cancellationToken);
        return AlbumHttp.From(result);
    }

    [HttpPost("{albumId:int}/proposals")]
    public async Task<IActionResult> Propose(
        int albumId,
        [FromBody] CreateProposalRequest body,
        CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        if (memberId is null)
        {
            return Unauthorized(new { error = "Sign in with Google first" });
        }

        var result = await mediator.Send(
            new CreateProposal.Command(albumId, memberId.Value, body.SongId),
            cancellationToken);
        return AlbumHttp.From(result, created: true);
    }

    [HttpGet("{albumId:int}/proposals/{proposalId:int}")]
    public async Task<IActionResult> GetProposal(
        int albumId,
        int proposalId,
        CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        if (memberId is null)
        {
            return Unauthorized(new { error = "Sign in with Google first" });
        }

        var result = await mediator.Send(
            new GetProposal.Query(albumId, proposalId, memberId.Value),
            cancellationToken);
        return AlbumHttp.From(result);
    }

    [HttpPost("{albumId:int}/proposals/{proposalId:int}/decisions")]
    public async Task<IActionResult> Decide(
        int albumId,
        int proposalId,
        [FromBody] ProposalDecisionRequest body,
        CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        if (memberId is null)
        {
            return Unauthorized(new { error = "Sign in with Google first" });
        }

        var result = await mediator.Send(
            new DecideProposal.Command(albumId, proposalId, memberId.Value, body.Decision),
            cancellationToken);
        return AlbumHttp.From(result);
    }

    [HttpPost("{albumId:int}/proposals/{proposalId:int}/withdraw")]
    public async Task<IActionResult> Withdraw(
        int albumId,
        int proposalId,
        CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        if (memberId is null)
        {
            return Unauthorized(new { error = "Sign in with Google first" });
        }

        var result = await mediator.Send(
            new WithdrawProposal.Command(albumId, proposalId, memberId.Value),
            cancellationToken);
        return AlbumHttp.From(result);
    }

    [HttpPost("{albumId:int}/proposals/{proposalId:int}/reviews")]
    public async Task<IActionResult> AddReview(
        int albumId,
        int proposalId,
        [FromBody] CreateProposalReviewRequest body,
        CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        if (memberId is null)
        {
            return Unauthorized(new { error = "Sign in with Google first" });
        }

        var result = await mediator.Send(
            new AddProposalReview.Command(
                albumId,
                proposalId,
                memberId.Value,
                body.StartMs,
                body.EndMs,
                body.Body),
            cancellationToken);
        return AlbumHttp.From(result, created: true);
    }

    [HttpDelete("{albumId:int}/proposals/{proposalId:int}/reviews/{reviewId:int}")]
    public async Task<IActionResult> RemoveReview(
        int albumId,
        int proposalId,
        int reviewId,
        CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        if (memberId is null)
        {
            return Unauthorized(new { error = "Sign in with Google first" });
        }

        var result = await mediator.Send(
            new RemoveProposalReview.Command(albumId, proposalId, reviewId, memberId.Value),
            cancellationToken);
        return AlbumHttp.From(result);
    }
}

internal static class AlbumHttp
{
    public static IActionResult From<T>(AlbumActionResult<T> result, bool created = false)
    {
        return result.Error switch
        {
            null when result.Value is not null && created => new CreatedResult("", result.Value),
            null when result.Value is not null => new OkObjectResult(result.Value),
            "not_in_band" or "not_found" or "song_missing" or "folder_missing" => new NotFoundResult(),
            "outside_folder" => new NotFoundObjectResult(new { error = "File is outside the band Drive folder" }),
            "forbidden" => new ObjectResult(new { error = "Owners and uploaders can manage albums" })
            {
                StatusCode = StatusCodes.Status403Forbidden
            },
            "forbidden_review" => new ObjectResult(new { error = "Only the author or a band owner can remove that review" })
            {
                StatusCode = StatusCodes.Status403Forbidden
            },
            "archived" => new ConflictObjectResult(new { error = "That album is archived" }),
            "already_on_album" => new ConflictObjectResult(new { error = "That take is already on the album" }),
            "already_open" => new ConflictObjectResult(new { error = "That take already has an open proposal" }),
            "not_open" => new ConflictObjectResult(new { error = "That proposal is no longer open" }),
            "invalid_name" => new BadRequestObjectResult(new { error = "Album name is required" }),
            "invalid_rule" => new BadRequestObjectResult(new { error = "Approval rule must be all or owner_uploaders" }),
            "invalid_decision" => new BadRequestObjectResult(new { error = "Decision must be approve or reject" }),
            "invalid_review" => new BadRequestObjectResult(new { error = "Review needs a comment and a start at or before the end" }),
            _ => new NotFoundResult()
        };
    }
}
