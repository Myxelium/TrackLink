using api.Contracts;
using api.Data;
using api.Data.Entities;
using api.Services;
using MediatR;

namespace api.Handlers.Albums;

public static class CastInclusionVote
{
    public record Command(int AlbumId, int MemberId, int SongId, string? Choice)
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

            var choice = request.Choice?.Trim().ToLowerInvariant();
            if (!AlbumInclusionChoices.IsKnown(choice))
            {
                return new AlbumActionResult<AlbumDetailDto>("invalid_inclusion", null);
            }

            if (album.Tracks.All(track => track.SongId != request.SongId))
            {
                return new AlbumActionResult<AlbumDetailDto>("not_on_album", null);
            }

            var existing = album.Votes.FirstOrDefault(vote =>
                vote.Kind == AlbumVoteKinds.Inclusion &&
                vote.MemberId == request.MemberId &&
                vote.SongId == request.SongId);
            if (existing is null)
            {
                album.Votes.Add(new Vote
                {
                    AlbumId = album.Id,
                    MemberId = request.MemberId,
                    SongId = request.SongId,
                    Kind = AlbumVoteKinds.Inclusion,
                    Choice = choice
                });
            }
            else
            {
                existing.Choice = choice;
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
