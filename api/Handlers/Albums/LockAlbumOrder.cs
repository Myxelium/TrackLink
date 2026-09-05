using api.Contracts;
using api.Data;
using api.Services;
using MediatR;

namespace api.Handlers.Albums;

public static class LockAlbumOrder
{
    public record Command(int AlbumId, int MemberId, bool Locked)
        : IRequest<AlbumActionResult<AlbumDetailDto>>;

    public class Handler(DatabaseContext db) : IRequestHandler<Command, AlbumActionResult<AlbumDetailDto>>
    {
        public async Task<AlbumActionResult<AlbumDetailDto>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var album = await LoadAlbum.ById(db, request.AlbumId, cancellationToken);
            if (album is null)
            {
                return new AlbumActionResult<AlbumDetailDto>("not_found", null);
            }

            if (album.Archived)
            {
                return new AlbumActionResult<AlbumDetailDto>("archived", null);
            }

            var members = await LoadAlbum.BandMembers(db, album.BandId, cancellationToken);
            var membership = members.FirstOrDefault(member => member.MemberId == request.MemberId);
            if (membership is null)
            {
                return new AlbumActionResult<AlbumDetailDto>("not_in_band", null);
            }

            if (!BandRoles.CanManageAlbums(membership.RoleName))
            {
                return new AlbumActionResult<AlbumDetailDto>("forbidden", null);
            }

            if (request.Locked && !album.OrderLocked)
            {
                AlbumOrderVotes.ApplyConsensusOrder(album);
            }

            album.OrderLocked = request.Locked;
            await db.SaveChangesAsync(cancellationToken);

            var refreshed = await LoadAlbum.ById(db, album.Id, cancellationToken);
            return new AlbumActionResult<AlbumDetailDto>(
                null,
                AlbumDtoMapper.ToDetail(refreshed!, members, request.MemberId));
        }
    }
}
