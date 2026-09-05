using api.Contracts;
using api.Data;
using api.Data.Entities;
using api.Integrations.Google;
using api.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace api.Handlers.Albums;

public static class GetAlbum
{
    public record Query(int AlbumId, int MemberId) : IRequest<AlbumActionResult<AlbumDetailDto>>;

    public class Handler(DatabaseContext db, IGoogleDriveService googleDrive)
        : IRequestHandler<Query, AlbumActionResult<AlbumDetailDto>>
    {
        public async Task<AlbumActionResult<AlbumDetailDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var album = await LoadAlbum.ById(db, request.AlbumId, cancellationToken);
            if (album is null)
            {
                return new AlbumActionResult<AlbumDetailDto>("not_found", null);
            }

            var members = await LoadAlbum.BandMembers(db, album.BandId, cancellationToken);
            if (members.All(member => member.MemberId != request.MemberId))
            {
                return new AlbumActionResult<AlbumDetailDto>("not_in_band", null);
            }

            var unsettled = await AlbumArtStorage.ClearIfMissingAsync(
                album,
                googleDrive,
                request.MemberId,
                cancellationToken);
            foreach (var proposal in album.Proposals.Where(item =>
                         item.Status is AlbumProposalStatuses.Open or AlbumProposalStatuses.Rejected))
            {
                var nextStatus = AlbumDtoMapper.Evaluate(proposal, album.ApprovalRule, members);
                if (nextStatus == proposal.Status &&
                    (nextStatus != AlbumProposalStatuses.Approved ||
                     album.Tracks.Any(track => track.SongId == proposal.SongId)))
                {
                    continue;
                }

                AlbumDtoMapper.AdmitIfApproved(album, proposal, nextStatus);
                unsettled = true;
            }

            if (unsettled)
            {
                await db.SaveChangesAsync(cancellationToken);
            }

            return new AlbumActionResult<AlbumDetailDto>(null, AlbumDtoMapper.ToDetail(album, members, request.MemberId));
        }
    }
}

public static class LoadAlbum
{
    public static Task<Album?> ById(DatabaseContext db, int albumId, CancellationToken cancellationToken)
    {
        return db.Albums
            .Include(album => album.Band)
            .Include(album => album.Tracks)
            .ThenInclude(track => track.Song)
            .Include(album => album.Votes)
            .Include(album => album.Proposals)
            .ThenInclude(proposal => proposal.Song)
            .Include(album => album.Proposals)
            .ThenInclude(proposal => proposal.ProposedByNavigation)
            .Include(album => album.Proposals)
            .ThenInclude(proposal => proposal.Decisions)
            .ThenInclude(decision => decision.Member)
            .Include(album => album.Proposals)
            .ThenInclude(proposal => proposal.Reviews)
            .ThenInclude(review => review.Member)
            .FirstOrDefaultAsync(album => album.Id == albumId, cancellationToken);
    }

    public static Task<List<BandMember>> BandMembers(DatabaseContext db, int bandId, CancellationToken cancellationToken)
    {
        return db.BandMembers
            .Include(member => member.Member)
            .ThenInclude(member => member.GoogleAccount)
            .Where(member => member.BandId == bandId)
            .ToListAsync(cancellationToken);
    }

    public static async Task<AlbumActionResult<AlbumProposalDto>> MapProposal(
        DatabaseContext db,
        AlbumProposal proposal,
        CancellationToken cancellationToken)
    {
        var members = await BandMembers(db, proposal.Album.BandId, cancellationToken);
        return new AlbumActionResult<AlbumProposalDto>(
            null,
            AlbumDtoMapper.ToProposal(proposal, proposal.Album.ApprovalRule, members));
    }
}
