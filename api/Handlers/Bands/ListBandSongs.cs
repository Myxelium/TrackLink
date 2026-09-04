using api.Contracts;
using api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace api.Handlers.Bands;

public static class ListBandSongs
{
    public record Query(int BandId) : IRequest<IReadOnlyList<SongDto>?>;

    public class Handler(DatabaseContext db) : IRequestHandler<Query, IReadOnlyList<SongDto>?>
    {
        public async Task<IReadOnlyList<SongDto>?> Handle(Query request, CancellationToken cancellationToken)
        {
            var exists = await db.Bands.AnyAsync(b => b.Id == request.BandId, cancellationToken);
            if (!exists)
            {
                return null;
            }

            return await db.SongIdentifiers
                .AsNoTracking()
                .Where(si => si.BandId == request.BandId)
                .Select(si => si.Song)
                .OrderBy(s => s.Name)
                .ThenBy(s => s.Version)
                .Select(s => new SongDto(
                    s.Id,
                    s.Name,
                    s.Description,
                    s.UploadedBy,
                    s.Version,
                    s.PreviousVersion,
                    s.Url.StartsWith("gdrive:") ? "gdrive" : "url"))
                .ToListAsync(cancellationToken);
        }
    }
}
