using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/songs")]
public class SongController(IAudioPlaybackService audio) : ControllerBase
{
    [HttpGet("{id:int}/audio")]
    public async Task<IActionResult> Play(int id, CancellationToken cancellationToken)
    {
        var opened = await audio.OpenAsync(id, cancellationToken);
        if (opened is null)
        {
            return NotFound();
        }

        Response.Headers.AcceptRanges = "bytes";
        return File(opened.Stream, opened.ContentType, enableRangeProcessing: opened.EnableRangeProcessing);
    }
}
