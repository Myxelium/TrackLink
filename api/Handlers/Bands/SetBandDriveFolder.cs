using api.Contracts;
using api.Data;
using api.Integrations.Google;
using api.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace api.Handlers.Bands;

public static class SetBandDriveFolder
{
    public record Command(int BandId, int MemberId, string FolderId, string? Name) : IRequest<BandSummaryDto?>;

    public class Handler(DatabaseContext db, IGoogleDriveService googleDrive) : IRequestHandler<Command, BandSummaryDto?>
    {
        public async Task<BandSummaryDto?> Handle(Command request, CancellationToken cancellationToken)
        {
            var membership = await db.BandMembers
                .Include(bandMember => bandMember.Band)
                .FirstOrDefaultAsync(
                    bandMember => bandMember.BandId == request.BandId && bandMember.MemberId == request.MemberId,
                    cancellationToken);
            if (membership is null || !BandRoles.CanSetFolder(membership.RoleName))
            {
                return null;
            }

            if (!await googleDrive.CanReadFolderAsync(request.MemberId, request.FolderId, cancellationToken))
            {
                return null;
            }

            membership.Band.DriveFolderId = request.FolderId;
            membership.Band.DriveFolderName = string.IsNullOrWhiteSpace(request.Name)
                ? membership.Band.DriveFolderName
                : request.Name.Trim();
            if (membership.Band.DriveFolderName is { Length: > 200 })
            {
                membership.Band.DriveFolderName = membership.Band.DriveFolderName[..200];
            }

            await db.SaveChangesAsync(cancellationToken);
            return new BandSummaryDto(
                membership.Band.Id,
                membership.Band.Name,
                membership.Band.Genre,
                membership.Band.Image,
                membership.Band.DriveFolderId,
                membership.Band.DriveFolderName,
                membership.RoleName,
                true);
        }
    }
}
