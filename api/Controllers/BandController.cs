using api.Contracts;
using api.Handlers.Albums;
using api.Handlers.Bands;
using api.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/bands")]
public class BandController(ISender mediator, IMemberSession memberSession) : ControllerBase
{
    [HttpGet("{bandId:int}/albums")]
    public async Task<IActionResult> Albums(int bandId, CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        if (memberId is null)
        {
            return Unauthorized(new { error = "Sign in with Google first" });
        }

        var result = await mediator.Send(new ListAlbums.Query(bandId, memberId.Value), cancellationToken);
        return AlbumHttp.From(result);
    }

    [HttpPost("{bandId:int}/albums")]
    public async Task<IActionResult> CreateAlbum(
        int bandId,
        [FromBody] CreateAlbumRequest body,
        CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        if (memberId is null)
        {
            return Unauthorized(new { error = "Sign in with Google first" });
        }

        var result = await mediator.Send(
            new CreateAlbum.Command(bandId, memberId.Value, body.Name, body.ApprovalRule),
            cancellationToken);
        return AlbumHttp.From(result, created: true);
    }

    [HttpGet("{bandId:int}/songs")]
    public async Task<IActionResult> Songs(int bandId, CancellationToken cancellationToken)
    {
        var songs = await mediator.Send(new ListBandSongs.Query(bandId), cancellationToken);
        return songs is null ? NotFound() : Ok(songs);
    }

    [HttpGet("{bandId:int}/drive/files")]
    public async Task<IActionResult> DriveFiles(int bandId, CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        if (memberId is null)
        {
            return Unauthorized(new { error = "Sign in with Google first" });
        }

        var result = await mediator.Send(new ListBandDriveFiles.Query(bandId, memberId.Value), cancellationToken);
        return result.Error switch
        {
            "not_in_band" => NotFound(),
            "folder_missing" => Conflict(new { error = "Band Drive folder is not set" }),
            "folder_denied" => StatusCode(
                StatusCodes.Status403Forbidden,
                new { error = "Ask the owner to share the band folder with your Google account" }),
            _ => Ok(result.Files)
        };
    }

    [HttpPut("{bandId:int}/drive-folder")]
    public async Task<IActionResult> SetDriveFolder(
        int bandId,
        [FromBody] SetDriveFolderRequest body,
        CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        if (memberId is null)
        {
            return Unauthorized(new { error = "Sign in with Google first" });
        }

        var folderId = body.FolderId?.Trim() ?? "";
        if (!IsSafeDriveFileId(folderId))
        {
            return BadRequest(new { error = "folderId is required" });
        }

        var band = await mediator.Send(
            new SetBandDriveFolder.Command(bandId, memberId.Value, folderId, body.Name),
            cancellationToken);
        return band is null ? NotFound() : Ok(band);
    }

    [HttpPost("{bandId:int}/invites")]
    public async Task<IActionResult> Invite(
        int bandId,
        [FromBody] CreateInviteRequest body,
        CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        if (memberId is null)
        {
            return Unauthorized(new { error = "Sign in with Google first" });
        }

        var invite = await mediator.Send(
            new CreateBandInvite.Command(bandId, memberId.Value, body.Email, body.Role),
            cancellationToken);
        return invite is null ? NotFound() : Ok(invite);
    }

    [HttpPost("{bandId:int}/songs")]
    public async Task<IActionResult> AddDriveSong(
        int bandId,
        [FromBody] AddDriveSongRequest body,
        CancellationToken cancellationToken)
    {
        var memberId = memberSession.GetMemberId(HttpContext);
        if (memberId is null)
        {
            return Unauthorized(new { error = "Sign in with Google first" });
        }

        var driveFileId = body.DriveFileId?.Trim() ?? "";
        if (!IsSafeDriveFileId(driveFileId))
        {
            return BadRequest(new { error = "driveFileId is required" });
        }

        var result = await mediator.Send(
            new AddBandDriveSong.Command(bandId, memberId.Value, driveFileId, body.Name),
            cancellationToken);

        return result.Error switch
        {
            "not_in_band" or "folder_missing" => NotFound(),
            "outside_folder" => NotFound(new { error = "File is outside the band Drive folder" }),
            "forbidden" => StatusCode(StatusCodes.Status403Forbidden, new { error = "Uploaders and owners can link takes" }),
            _ when result.Song is not null => Created($"/api/songs/{result.Song.Id}/audio", result.Song),
            _ => NotFound()
        };
    }

    private static bool IsSafeDriveFileId(string driveFileId)
    {
        return driveFileId.Length is > 0 and <= 128
            && driveFileId.All(character => char.IsLetterOrDigit(character) || character is '_' or '-');
    }
}
