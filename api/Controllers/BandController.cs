using api.Handlers.Bands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/bands")]
public class BandController(ISender mediator) : ControllerBase
{
    [HttpGet("{bandId:int}/songs")]
    public async Task<IActionResult> Songs(int bandId, CancellationToken cancellationToken)
    {
        var songs = await mediator.Send(new ListBandSongs.Query(bandId), cancellationToken);
        return songs is null ? NotFound() : Ok(songs);
    }
}
