using api.Contracts;
using api.Data;
using api.Data.Entities;
using api.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace api.Handlers.Albums;

public static class CreateAlbum
{
    public record Command(int BandId, int MemberId, string Name, string? ApprovalRule)
        : IRequest<AlbumActionResult<AlbumSummaryDto>>;

    public class Handler(DatabaseContext db) : IRequestHandler<Command, AlbumActionResult<AlbumSummaryDto>>
    {
        public async Task<AlbumActionResult<AlbumSummaryDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var membership = await db.BandMembers
                .FirstOrDefaultAsync(
                    bandMember => bandMember.BandId == request.BandId && bandMember.MemberId == request.MemberId,
                    cancellationToken);
            if (membership is null)
            {
                return new AlbumActionResult<AlbumSummaryDto>("not_in_band", null);
            }

            if (!BandRoles.CanManageAlbums(membership.RoleName))
            {
                return new AlbumActionResult<AlbumSummaryDto>("forbidden", null);
            }

            var name = request.Name.Trim();
            if (name.Length is 0 or > 50)
            {
                return new AlbumActionResult<AlbumSummaryDto>("invalid_name", null);
            }

            var album = new Album
            {
                BandId = request.BandId,
                Name = name,
                Archived = false,
                ApprovalRule = AlbumApproval.Normalize(request.ApprovalRule),
                CreatedBy = request.MemberId,
                CreatedDate = DateTime.UtcNow
            };
            db.Albums.Add(album);
            await db.SaveChangesAsync(cancellationToken);

            return new AlbumActionResult<AlbumSummaryDto>(null, AlbumDtoMapper.ToSummary(album));
        }
    }
}
