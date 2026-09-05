using api.Contracts;
using api.Data;
using api.Data.Entities;
using api.Integrations.Google;
using api.Services;
using MediatR;

namespace api.Handlers.Albums;

public static class CastArtVote
{
    public record Command(int AlbumId, int MemberId, string? DriveFileId)
        : IRequest<AlbumActionResult<AlbumDetailDto>>;

    public class Handler(DatabaseContext db, IGoogleDriveService googleDrive)
        : IRequestHandler<Command, AlbumActionResult<AlbumDetailDto>>
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

            if (album.ArtLocked)
            {
                return new AlbumActionResult<AlbumDetailDto>("art_locked", null);
            }

            var members = await LoadAlbum.BandMembers(db, album.BandId, cancellationToken);
            if (members.All(member => member.MemberId != request.MemberId))
            {
                return new AlbumActionResult<AlbumDetailDto>("not_in_band", null);
            }

            var driveFileId = AlbumArtVotes.Normalize(request.DriveFileId);
            if (driveFileId is null)
            {
                return new AlbumActionResult<AlbumDetailDto>("invalid_art", null);
            }

            var folderId = album.Band.DriveFolderId;
            if (string.IsNullOrWhiteSpace(folderId))
            {
                return new AlbumActionResult<AlbumDetailDto>("folder_missing", null);
            }

            if (!await googleDrive.IsFileInsideFolderAsync(
                    request.MemberId,
                    driveFileId,
                    folderId,
                    cancellationToken))
            {
                return new AlbumActionResult<AlbumDetailDto>("outside_folder", null);
            }

            var meta = await googleDrive.GetAudioFileAsync(request.MemberId, driveFileId, cancellationToken);
            if (!DriveFileKinds.IsImageMime(meta?.MimeType))
            {
                return new AlbumActionResult<AlbumDetailDto>("not_image", null);
            }

            var existing = album.Votes.FirstOrDefault(vote =>
                vote.Kind == AlbumVoteKinds.Art &&
                vote.MemberId == request.MemberId &&
                vote.SongId is null);
            if (existing is null)
            {
                album.Votes.Add(new Vote
                {
                    AlbumId = album.Id,
                    MemberId = request.MemberId,
                    Kind = AlbumVoteKinds.Art,
                    Subject = driveFileId
                });
            }
            else
            {
                existing.Subject = driveFileId;
                existing.AlbumId = album.Id;
            }

            await db.SaveChangesAsync(cancellationToken);

            var refreshed = await LoadAlbum.ById(db, album.Id, cancellationToken);
            return new AlbumActionResult<AlbumDetailDto>(
                null,
                AlbumDtoMapper.ToDetail(refreshed!, members, request.MemberId));
        }
    }
}
