using api.Contracts;
using api.Data;
using api.Data.Entities;
using api.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace api.Handlers.Albums;

public static class DecideProposal
{
    public record Command(int AlbumId, int ProposalId, int MemberId, string Decision)
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

            if (proposal.Status is AlbumProposalStatuses.Approved or AlbumProposalStatuses.Withdrawn)
            {
                return new AlbumActionResult<AlbumProposalDto>("not_open", null);
            }

            var members = await LoadAlbum.BandMembers(db, album.BandId, cancellationToken);
            if (members.All(member => member.MemberId != request.MemberId))
            {
                return new AlbumActionResult<AlbumProposalDto>("not_in_band", null);
            }

            var decision = request.Decision.Trim().ToLowerInvariant();
            if (!AlbumProposalDecisions.IsKnown(decision))
            {
                return new AlbumActionResult<AlbumProposalDto>("invalid_decision", null);
            }

            var existing = proposal.Decisions.FirstOrDefault(item => item.MemberId == request.MemberId);
            if (existing is null)
            {
                existing = new AlbumProposalDecision
                {
                    ProposalId = proposal.Id,
                    MemberId = request.MemberId,
                    Decision = decision,
                    UpdatedAt = DateTime.UtcNow,
                    Member = members.First(member => member.MemberId == request.MemberId).Member
                };
                proposal.Decisions.Add(existing);
                db.AlbumProposalDecisions.Add(existing);
            }
            else
            {
                existing.Decision = decision;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            AlbumDtoMapper.AdmitIfApproved(
                proposal: proposal,
                album: album,
                nextStatus: AlbumDtoMapper.Evaluate(proposal, album.ApprovalRule, members));

            await db.SaveChangesAsync(cancellationToken);
            return await LoadAlbum.MapProposal(db, proposal, cancellationToken);
        }
    }
}
