using api.Contracts;
using api.Data;
using api.Data.Entities;
using api.Services;
using MediatR;

namespace api.Handlers.Albums;

public static class AddProposalReview
{
    public record Command(int AlbumId, int ProposalId, int MemberId, int StartMs, int? EndMs, string Body)
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

            if (proposal.Status is AlbumProposalStatuses.Withdrawn)
            {
                return new AlbumActionResult<AlbumProposalDto>("not_open", null);
            }

            var members = await LoadAlbum.BandMembers(db, album.BandId, cancellationToken);
            var membership = members.FirstOrDefault(member => member.MemberId == request.MemberId);
            if (membership is null)
            {
                return new AlbumActionResult<AlbumProposalDto>("not_in_band", null);
            }

            var body = request.Body?.Trim() ?? string.Empty;
            var endMs = request.EndMs ?? request.StartMs;
            if (string.IsNullOrWhiteSpace(body) || body.Length > 2000 ||
                request.StartMs < 0 || endMs < request.StartMs)
            {
                return new AlbumActionResult<AlbumProposalDto>("invalid_review", null);
            }

            proposal.Reviews.Add(new AlbumProposalReview
            {
                ProposalId = proposal.Id,
                MemberId = request.MemberId,
                StartMs = request.StartMs,
                EndMs = endMs,
                Body = body,
                CreatedAt = DateTime.UtcNow,
                Member = membership.Member
            });
            await db.SaveChangesAsync(cancellationToken);
            return await LoadAlbum.MapProposal(db, proposal, cancellationToken);
        }
    }
}
