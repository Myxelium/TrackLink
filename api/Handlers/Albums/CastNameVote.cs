using api.Contracts;
using api.Data;
using api.Data.Entities;
using api.Services;
using MediatR;

namespace api.Handlers.Albums;

public static class CastNameVote
{
    public record Command(int AlbumId, int MemberId, int? SongId, string? Name)
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
            if (members.All(member => member.MemberId != request.MemberId))
            {
                return new AlbumActionResult<AlbumDetailDto>("not_in_band", null);
            }

            var name = AlbumNameVotes.Normalize(request.Name);
            if (name is null)
            {
                return new AlbumActionResult<AlbumDetailDto>("invalid_title", null);
            }

            var isSongTitle = request.SongId is not null;
            if (isSongTitle && album.Tracks.All(track => track.SongId != request.SongId))
            {
                return new AlbumActionResult<AlbumDetailDto>("not_on_album", null);
            }

            var kind = isSongTitle ? AlbumVoteKinds.SongName : AlbumVoteKinds.AlbumName;
            var officialName = isSongTitle
                ? album.Tracks.First(track => track.SongId == request.SongId).Song.Name
                : album.Name;
            var existingSpellings = album.Votes
                .Where(vote => vote.Kind == kind &&
                               !string.IsNullOrWhiteSpace(vote.Subject) &&
                               (isSongTitle ? vote.SongId == request.SongId : vote.SongId is null))
                .Select(vote => vote.Subject!)
                .Prepend(officialName);
            var subject = AlbumNameVotes.CanonicalName(existingSpellings, name);

            var existing = album.Votes.FirstOrDefault(vote =>
                vote.Kind == kind &&
                vote.MemberId == request.MemberId &&
                (isSongTitle ? vote.SongId == request.SongId : vote.SongId is null));
            if (existing is null)
            {
                album.Votes.Add(new Vote
                {
                    AlbumId = album.Id,
                    MemberId = request.MemberId,
                    SongId = request.SongId,
                    Kind = kind,
                    Subject = subject
                });
            }
            else
            {
                existing.Subject = subject;
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
