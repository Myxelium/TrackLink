using api.Contracts;
using api.Data;
using api.Integrations.Google;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace api.Handlers.Bands;

public static class ListBandDriveFiles
{
    public record Query(int BandId, int MemberId) : IRequest<DriveListResult>;

    public class Handler(DatabaseContext db, IGoogleDriveService googleDrive)
        : IRequestHandler<Query, DriveListResult>
    {
        public async Task<DriveListResult> Handle(Query request, CancellationToken cancellationToken)
        {
            var membership = await db.BandMembers
                .Include(bandMember => bandMember.Band)
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    bandMember => bandMember.BandId == request.BandId && bandMember.MemberId == request.MemberId,
                    cancellationToken);
            if (membership is null)
            {
                return new DriveListResult("not_in_band", []);
            }

            var folderId = membership.Band.DriveFolderId;
            if (string.IsNullOrWhiteSpace(folderId))
            {
                return new DriveListResult("folder_missing", []);
            }

            try
            {
                var files = await googleDrive.ListAudioFilesAsync(request.MemberId, folderId, cancellationToken);
                return new DriveListResult(null, files);
            }
            catch (DriveFolderDeniedException)
            {
                return new DriveListResult("folder_denied", []);
            }
        }
    }
}

public record DriveListResult(string? Error, IReadOnlyList<DriveFileDto> Files);
