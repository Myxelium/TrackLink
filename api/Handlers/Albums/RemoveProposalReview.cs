using api.Contracts;
using api.Data;
using api.Services;
using MediatR;

namespace api.Handlers.Albums;

public static class RemoveProposalReview
{
    public record Command(int AlbumId, int ProposalId, int ReviewId, int MemberId)
        : IRequest<AlbumActionResult<AlbumProposalDto>>;

    public class Handler(DatabaseContext db) : IRequestHandler<Command, AlbumActionResult<AlbumProposalDto>>
    {
        public async Task<AlbumActionResult<AlbumProposalDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var album = await LoadAlbum.ById(db, request.AlbumId, cancellationToken);
            if (album is null)
            {
                return new AlbumActionResult<AlbumProposalDto>("not_found", null);
            }

            var proposal = album.Proposals.FirstOrDefault(item => item.Id == request.ProposalId);
            if (proposal is null)
            {
                return new AlbumActionResult<AlbumProposalDto>("not_found", null);
            }

            var review = proposal.Reviews.FirstOrDefault(item => item.Id == request.ReviewId);
            if (review is null)
            {
                return new AlbumActionResult<AlbumProposalDto>("not_found", null);
            }

            var members = await LoadAlbum.BandMembers(db, album.BandId, cancellationToken);
            var membership = members.FirstOrDefault(member => member.MemberId == request.MemberId);
            if (membership is null)
            {
                return new AlbumActionResult<AlbumProposalDto>("not_in_band", null);
            }

            if (review.MemberId != request.MemberId && membership.RoleName != BandRoles.Owner)
            {
                return new AlbumActionResult<AlbumProposalDto>("forbidden_review", null);
            }

            db.AlbumProposalReviews.Remove(review);
            await db.SaveChangesAsync(cancellationToken);
            return await LoadAlbum.MapProposal(db, proposal, cancellationToken);
        }
    }
}
