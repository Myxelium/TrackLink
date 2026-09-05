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
    [HttpGet("{albumId:int}/art")]
    public async Task<IActionResult> Art(
        int albumId,
        [FromQuery] string? fileId,
        CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        if (memberId is null)
        {
            return Unauthorized(new { error = "Sign in with Google first" });
        }

        var result = await mediator.Send(
            new GetAlbumArt.Query(albumId, memberId.Value, fileId),
            cancellationToken);
        if (result.Error is null && result.Stream is not null && result.ContentType is not null)
        {
            return File(result.Stream, result.ContentType);
        }

        return NotFound();
    }

    [HttpPost("{albumId:int}/art")]
    [RequestSizeLimit(AlbumArtPixels.MaxBytes + 65_536)]
    [RequestFormLimits(MultipartBodyLengthLimit = AlbumArtPixels.MaxBytes + 65_536)]
    public async Task<IActionResult> UploadArt(
        int albumId,
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        if (memberId is null)
        {
            return Unauthorized(new { error = "Sign in with Google first" });
        }

        if (file is null || file.Length <= 0)
        {
            return BadRequest(new { error = "Cover image is required" });
        }

        try
        {
            await using var content = file.OpenReadStream();
            var result = await mediator.Send(
                new UploadAlbumArt.Command(
                    albumId,
                    memberId.Value,
                    file.FileName,
                    file.ContentType,
                    content,
                    file.Length),
                cancellationToken);
            return AlbumHttp.From(result, created: true);
        }
        catch (Exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "Could not upload that image to the band Drive folder" });
        }
    }

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

    [HttpPut("{albumId:int}/inclusion-votes")]
    public async Task<IActionResult> CastInclusion(
        int albumId,
        [FromBody] InclusionVoteRequest body,
        CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        if (memberId is null)
        {
            return Unauthorized(new { error = "Sign in with Google first" });
        }

        var result = await mediator.Send(
            new CastInclusionVote.Command(albumId, memberId.Value, body.SongId, body.Choice),
            cancellationToken);
        return AlbumHttp.From(result);
    }

    [HttpPut("{albumId:int}/name-votes")]
    public async Task<IActionResult> CastName(
        int albumId,
        [FromBody] NameVoteRequest body,
        CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        if (memberId is null)
        {
            return Unauthorized(new { error = "Sign in with Google first" });
        }

        var result = await mediator.Send(
            new CastNameVote.Command(albumId, memberId.Value, body.SongId, body.Name),
            cancellationToken);
        return AlbumHttp.From(result);
    }

    [HttpPut("{albumId:int}/order-votes")]
    public async Task<IActionResult> CastOrder(
        int albumId,
        [FromBody] OrderVoteRequest body,
        CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        if (memberId is null)
        {
            return Unauthorized(new { error = "Sign in with Google first" });
        }

        var result = await mediator.Send(
            new CastOrderVote.Command(albumId, memberId.Value, body.SongIds),
            cancellationToken);
        return AlbumHttp.From(result);
    }

    [HttpPut("{albumId:int}/order-lock")]
    public async Task<IActionResult> LockOrder(
        int albumId,
        [FromBody] OrderLockRequest body,
        CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        if (memberId is null)
        {
            return Unauthorized(new { error = "Sign in with Google first" });
        }

        var result = await mediator.Send(
            new LockAlbumOrder.Command(albumId, memberId.Value, body.Locked),
            cancellationToken);
        return AlbumHttp.From(result);
    }

    [HttpPut("{albumId:int}/art-votes")]
    public async Task<IActionResult> CastArt(
        int albumId,
        [FromBody] ArtVoteRequest body,
        CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        if (memberId is null)
        {
            return Unauthorized(new { error = "Sign in with Google first" });
        }

        var result = await mediator.Send(
            new CastArtVote.Command(albumId, memberId.Value, body.DriveFileId),
            cancellationToken);
        return AlbumHttp.From(result);
    }

    [HttpPut("{albumId:int}/art-lock")]
    public async Task<IActionResult> LockArt(
        int albumId,
        [FromBody] ArtLockRequest body,
        CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        if (memberId is null)
        {
            return Unauthorized(new { error = "Sign in with Google first" });
        }

        var result = await mediator.Send(
            new LockAlbumArt.Command(albumId, memberId.Value, body.Locked),
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
            "not_on_album" => new ConflictObjectResult(new { error = "That take is not on this album" }),
            "invalid_inclusion" => new BadRequestObjectResult(new { error = "Choice must be in, out, or abstain" }),
            "invalid_title" => new BadRequestObjectResult(new { error = "Title must be 1 to 50 characters" }),
            "invalid_order" => new BadRequestObjectResult(new { error = "Order must rank every admitted take once" }),
            "order_locked" => new ConflictObjectResult(new { error = "The track order is locked" }),
            "invalid_art" => new BadRequestObjectResult(new { error = "Cover art must be a Drive file id" }),
            "not_image" => new BadRequestObjectResult(new { error = "Cover art must be an image in the band Drive folder" }),
            "empty_file" => new BadRequestObjectResult(new { error = "Cover image is required" }),
            "too_large" => new BadRequestObjectResult(new { error = "Cover image must be 10 MB or smaller" }),
            "upload_failed" => new ConflictObjectResult(new { error = "Could not upload that image to the band Drive folder" }),
            "needs_reauth" => new ObjectResult(new { error = "Sign in with Google again so TrackLink can write album art to Drive." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            },
            "art_locked" => new ConflictObjectResult(new { error = "The album art is locked" }),
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
