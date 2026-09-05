using api.Contracts;
using api.Data;
using api.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace api.Handlers.Albums;

public static class ListAlbums
{
    public record Query(int BandId, int MemberId) : IRequest<AlbumActionResult<IReadOnlyList<AlbumSummaryDto>>>;

    public class Handler(DatabaseContext db) : IRequestHandler<Query, AlbumActionResult<IReadOnlyList<AlbumSummaryDto>>>
    {
        public async Task<AlbumActionResult<IReadOnlyList<AlbumSummaryDto>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var inBand = await db.BandMembers.AnyAsync(
                bandMember => bandMember.BandId == request.BandId && bandMember.MemberId == request.MemberId,
                cancellationToken);
            if (!inBand)
            {
                return new AlbumActionResult<IReadOnlyList<AlbumSummaryDto>>("not_in_band", null);
            }

            var albums = await db.Albums
                .AsNoTracking()
                .Include(album => album.Tracks)
                .Include(album => album.Proposals)
                .Where(album => album.BandId == request.BandId)
                .OrderBy(album => album.Archived)
                .ThenBy(album => album.Name)
                .ToListAsync(cancellationToken);

            return new AlbumActionResult<IReadOnlyList<AlbumSummaryDto>>(
                null,
                albums.Select(AlbumDtoMapper.ToSummary).ToList());
        }
    }
}
