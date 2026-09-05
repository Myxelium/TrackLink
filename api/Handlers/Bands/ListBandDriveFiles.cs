using api.Contracts;
using api.Data;
using api.Integrations.Google;
using api.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace api.Handlers.Bands;

public static class ListBandDriveFiles
{
    public record Query(int BandId, int MemberId, string? Kind = null) : IRequest<DriveListResult>;

    public class Handler(DatabaseContext db, IGoogleDriveService googleDrive)
        : IRequestHandler<Query, DriveListResult>
    {
        public async Task<DriveListResult> Handle(Query request, CancellationToken cancellationToken)
        {
            var listKind = DriveFileKinds.Normalize(request.Kind);
            if (listKind is null)
            {
                return new DriveListResult("invalid_kind", []);
            }

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
                var files = listKind == DriveFileKinds.Image
                    ? await googleDrive.ListImageFilesAsync(request.MemberId, folderId, cancellationToken)
                    : await googleDrive.ListAudioFilesAsync(request.MemberId, folderId, cancellationToken);
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
