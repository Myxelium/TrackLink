using api.Contracts;
using api.Data;
using api.Data.Entities;
using api.Services;
using MediatR;

namespace api.Handlers.Albums;

public static class CastOrderVote
{
    public record Command(int AlbumId, int MemberId, IReadOnlyList<int>? SongIds)
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

            if (album.OrderLocked)
            {
                return new AlbumActionResult<AlbumDetailDto>("order_locked", null);
            }

            var members = await LoadAlbum.BandMembers(db, album.BandId, cancellationToken);
            if (members.All(member => member.MemberId != request.MemberId))
            {
                return new AlbumActionResult<AlbumDetailDto>("not_in_band", null);
            }

            var songIds = request.SongIds ?? [];
            var admittedIds = album.Tracks.Select(track => track.SongId).ToHashSet();
            if (admittedIds.Count == 0 || !AlbumOrderVotes.IsCompletePermutation(songIds, admittedIds))
            {
                return new AlbumActionResult<AlbumDetailDto>("invalid_order", null);
            }

            var stale = album.Votes
                .Where(vote => vote.Kind == AlbumVoteKinds.Order && vote.MemberId == request.MemberId)
                .ToList();
            foreach (var vote in stale)
            {
                album.Votes.Remove(vote);
                db.Votes.Remove(vote);
            }

            for (var index = 0; index < songIds.Count; index++)
            {
                album.Votes.Add(new Vote
                {
                    AlbumId = album.Id,
                    MemberId = request.MemberId,
                    SongId = songIds[index],
                    Kind = AlbumVoteKinds.Order,
                    Choice = AlbumOrderVotes.RankChoice(index + 1)
                });
            }

            await db.SaveChangesAsync(cancellationToken);

            var refreshed = await LoadAlbum.ById(db, album.Id, cancellationToken);
            return new AlbumActionResult<AlbumDetailDto>(
                null,
                AlbumDtoMapper.ToDetail(refreshed!, members, request.MemberId));
        }
    }
}
