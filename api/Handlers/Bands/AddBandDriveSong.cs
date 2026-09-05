using api.Contracts;
using api.Data;
using api.Data.Entities;
using api.Integrations.Google;
using api.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace api.Handlers.Bands;

public static class AddBandDriveSong
{
    public record Command(int BandId, int MemberId, string DriveFileId, string? Name) : IRequest<AddDriveSongResult>;

    public class Handler(DatabaseContext db, IGoogleDriveService googleDrive)
        : IRequestHandler<Command, AddDriveSongResult>
    {
        public async Task<AddDriveSongResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var membership = await db.BandMembers
                .Include(bandMember => bandMember.Band)
                .FirstOrDefaultAsync(
                    bandMember => bandMember.BandId == request.BandId && bandMember.MemberId == request.MemberId,
                    cancellationToken);
            if (membership is null)
            {
                return new AddDriveSongResult("not_in_band", null);
            }

            if (!BandRoles.CanUpload(membership.RoleName))
            {
                return new AddDriveSongResult("forbidden", null);
            }

            var folderId = membership.Band.DriveFolderId;
            if (string.IsNullOrWhiteSpace(folderId))
            {
                return new AddDriveSongResult("folder_missing", null);
            }

            if (!await googleDrive.IsFileInsideFolderAsync(
                    request.MemberId,
                    request.DriveFileId,
                    folderId,
                    cancellationToken))
            {
                return new AddDriveSongResult("outside_folder", null);
            }

            var songName = string.IsNullOrWhiteSpace(request.Name)
                ? "Drive take"
                : request.Name.Trim();
            if (songName.Length > 50)
            {
                songName = songName[..50];
            }

            var pointer = $"{AudioPlaybackService.DrivePrefix}{request.DriveFileId}";
            var catalog = await db.SongIdentifiers
                .Where(identifier => identifier.BandId == request.BandId)
                .Select(identifier => identifier.Song)
                .ToListAsync(cancellationToken);

            var alreadyLinked = catalog.FirstOrDefault(song =>
                song.Url.Equals(pointer, StringComparison.OrdinalIgnoreCase));
            if (alreadyLinked is not null)
            {
                return new AddDriveSongResult(null, ToDto(alreadyLinked));
            }

            var driveFile = await googleDrive.GetAudioFileAsync(
                request.MemberId,
                request.DriveFileId,
                cancellationToken);
            var priorTake = catalog
                .Where(song => song.Name.Equals(songName, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(song => song.Version ?? 0)
                .ThenByDescending(song => song.Id)
                .FirstOrDefault();

            var song = new Song
            {
                Name = songName,
                Description = "Linked from Google Drive",
                UploadedBy = request.MemberId,
                Version = (priorTake?.Version ?? 0) + 1,
                PreviousVersion = priorTake?.Id ?? 0,
                ContentMd5 = NormalizeMd5(driveFile?.Md5),
                SourceModifiedAt = driveFile?.ModifiedAt,
                Url = pointer
            };

            db.Songs.Add(song);
            await db.SaveChangesAsync(cancellationToken);

            db.SongIdentifiers.Add(new SongIdentifier
            {
                BandId = request.BandId,
                SongId = song.Id
            });
            await db.SaveChangesAsync(cancellationToken);

            return new AddDriveSongResult(null, ToDto(song));
        }

        private static SongDto ToDto(Song song)
        {
            return new SongDto(
                song.Id,
                song.Name,
                song.Description,
                song.UploadedBy,
                song.Version,
                song.PreviousVersion,
                "gdrive",
                song.ContentMd5,
                song.SourceModifiedAt);
        }

        private static string? NormalizeMd5(string? md5)
        {
            if (string.IsNullOrWhiteSpace(md5))
            {
                return null;
            }

            var trimmed = md5.Trim();
            return trimmed.Length > 32 ? trimmed[..32] : trimmed;
        }
    }
}

public record AddDriveSongResult(string? Error, SongDto? Song);
