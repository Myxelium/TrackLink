using api.Contracts;
using api.Data;
using api.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace api.Handlers.Albums;

public static class UpdateAlbum
{
    public record Command(int AlbumId, int MemberId, string? Name, bool? Archived, string? ApprovalRule)
        : IRequest<AlbumActionResult<AlbumDetailDto>>;

    public class Handler(DatabaseContext db) : IRequestHandler<Command, AlbumActionResult<AlbumDetailDto>>
    {
        public async Task<AlbumActionResult<AlbumDetailDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var album = await LoadAlbum.ById(db, request.AlbumId, cancellationToken);
            if (album is null)
            {
                return new AlbumActionResult<AlbumDetailDto>("not_found", null);
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

            if (request.Name is not null)
            {
                var name = request.Name.Trim();
                if (name.Length is 0 or > 50)
                {
                    return new AlbumActionResult<AlbumDetailDto>("invalid_name", null);
                }

                album.Name = name;
            }

            if (request.Archived is not null)
            {
                album.Archived = request.Archived.Value;
            }

            if (request.ApprovalRule is not null)
            {
                if (!AlbumApproval.IsKnown(request.ApprovalRule))
                {
                    return new AlbumActionResult<AlbumDetailDto>("invalid_rule", null);
                }

                album.ApprovalRule = request.ApprovalRule;
            }

            await db.SaveChangesAsync(cancellationToken);
            await db.Entry(album).ReloadAsync(cancellationToken);

            var refreshed = await LoadAlbum.ById(db, album.Id, cancellationToken);
            return new AlbumActionResult<AlbumDetailDto>(
                null,
                AlbumDtoMapper.ToDetail(refreshed!, members));
        }
    }
}
