using api.Contracts;
using api.Data;
using api.Data.Entities;
using api.Integrations.Google;
using api.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace api.Handlers.Bands;

public static class ListBandSongs
{
    public record Query(int BandId, int? MemberId, string? SearchQuery = null) : IRequest<IReadOnlyList<SongDto>?>;

    public class Handler(DatabaseContext db, IGoogleDriveService googleDrive)
        : IRequestHandler<Query, IReadOnlyList<SongDto>?>
    {
        public async Task<IReadOnlyList<SongDto>?> Handle(Query request, CancellationToken cancellationToken)
        {
            var band = await db.Bands
                .AsNoTracking()
                .FirstOrDefaultAsync(catalog => catalog.Id == request.BandId, cancellationToken);
            if (band is null)
            {
                return null;
            }

            var songs = await db.SongIdentifiers
                .AsNoTracking()
                .Where(identifier => identifier.BandId == request.BandId)
                .Select(identifier => identifier.Song)
                .OrderBy(song => song.Name)
                .ThenBy(song => song.Version)
                .ToListAsync(cancellationToken);

            var folderFileIds = await FolderFileIds(request.MemberId, band.DriveFolderId, cancellationToken);

            return songs
                .Where(song => IsVisibleTake(song, folderFileIds) && MatchesSearch(song, request.SearchQuery))
                .Select(song => new SongDto(
                    song.Id,
                    song.Name,
                    song.Description,
                    song.UploadedBy,
                    song.Version,
                    song.PreviousVersion,
                    song.Url.StartsWith(AudioPlaybackService.DrivePrefix, StringComparison.OrdinalIgnoreCase)
                        ? "gdrive"
                        : "url",
                    song.ContentMd5,
                    song.SourceModifiedAt))
                .ToList();
        }

        private async Task<HashSet<string>?> FolderFileIds(
            int? memberId,
            string? folderId,
            CancellationToken cancellationToken)
        {
            if (memberId is null || string.IsNullOrWhiteSpace(folderId))
            {
                return null;
            }

            try
            {
                var files = await googleDrive.ListAudioFilesAsync(memberId.Value, folderId, cancellationToken);
                return files
                    .Select(file => file.Id)
                    .ToHashSet(StringComparer.Ordinal);
            }
            catch (DriveFolderDeniedException)
            {
                return [];
            }
        }

        private static bool MatchesSearch(Song song, string? searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return true;
            }

            var needle = searchQuery.Trim();
            return song.Name.Contains(needle, StringComparison.OrdinalIgnoreCase)
                || (song.Description?.Contains(needle, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        private static bool IsVisibleTake(Song song, HashSet<string>? folderFileIds)
        {
            if (!song.Url.StartsWith(AudioPlaybackService.DrivePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (folderFileIds is null)
            {
                return false;
            }

            var fileId = song.Url[AudioPlaybackService.DrivePrefix.Length..];
            return folderFileIds.Contains(fileId);
        }
    }
}
