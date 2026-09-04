using api.Contracts;
using api.Data.Entities;

namespace api.Services;

public static class AlbumDtoMapper
{
    public static AlbumSummaryDto ToSummary(Album album)
    {
        return new AlbumSummaryDto(
            album.Id,
            album.BandId,
            album.Name,
            album.Archived,
            album.ApprovalRule,
            album.Tracks.Count,
            album.Proposals.Count(proposal => proposal.Status == AlbumProposalStatuses.Open));
    }

    public static AlbumDetailDto ToDetail(Album album, IReadOnlyList<BandMember> members)
    {
        return new AlbumDetailDto(
            album.Id,
            album.BandId,
            album.Name,
            album.Archived,
            album.ApprovalRule,
            album.Tracks
                .OrderBy(track => track.SortOrder)
                .Select(track => new AlbumTrackDto(
                    track.Id,
                    track.SongId,
                    track.Song.Name,
                    track.Song.Version,
                    track.SortOrder,
                    track.AddedAt))
                .ToList(),
            album.Proposals
                .OrderByDescending(proposal => proposal.CreatedAt)
                .Select(proposal => ToProposal(proposal, album.ApprovalRule, members))
                .ToList());
    }

    public static AlbumProposalDto ToProposal(
        AlbumProposal proposal,
        string approvalRule,
        IReadOnlyList<BandMember> members)
    {
        var required = AlbumApproval.RequiredMembers(approvalRule, members);
        var decisions = proposal.Decisions
            .OrderBy(decision => decision.UpdatedAt)
            .Select(decision => new ProposalDecisionDto(
                decision.MemberId,
                MemberLabel(decision.Member),
                decision.Decision,
                decision.UpdatedAt))
            .ToList();
        var approvedIds = proposal.Decisions
            .Where(decision => decision.Decision == AlbumProposalDecisions.Approve)
            .Select(decision => decision.MemberId)
            .ToHashSet();
        var waitingOn = required
            .Where(member => !approvedIds.Contains(member.MemberId))
            .Select(member => new WaitingMemberDto(member.MemberId, MemberLabel(member.Member)))
            .ToList();

        var reviews = proposal.Reviews
            .OrderBy(review => review.StartMs)
            .ThenBy(review => review.CreatedAt)
            .Select(review => new ProposalReviewDto(
                review.Id,
                review.MemberId,
                MemberLabel(review.Member),
                review.StartMs,
                review.EndMs,
                review.Body,
                review.CreatedAt))
            .ToList();

        return new AlbumProposalDto(
            proposal.Id,
            proposal.AlbumId,
            proposal.SongId,
            proposal.Song.Name,
            proposal.Song.Version,
            proposal.ProposedBy,
            MemberLabel(proposal.ProposedByNavigation),
            proposal.Status,
            proposal.CreatedAt,
            proposal.ResolvedAt,
            decisions,
            waitingOn,
            reviews);
    }

    public static string Evaluate(AlbumProposal proposal, string approvalRule, IReadOnlyList<BandMember> members)
    {
        var required = AlbumApproval.RequiredMembers(approvalRule, members);
        if (required.Count == 0)
        {
            return AlbumProposalStatuses.Open;
        }

        var byMember = proposal.Decisions.ToDictionary(decision => decision.MemberId);
        if (required.Any(member =>
                byMember.TryGetValue(member.MemberId, out var decision) &&
                decision.Decision == AlbumProposalDecisions.Reject))
        {
            return AlbumProposalStatuses.Rejected;
        }

        if (required.All(member =>
                byMember.TryGetValue(member.MemberId, out var decision) &&
                decision.Decision == AlbumProposalDecisions.Approve))
        {
            return AlbumProposalStatuses.Approved;
        }

        return AlbumProposalStatuses.Open;
    }

    public static void ApplyStatus(AlbumProposal proposal, string nextStatus)
    {
        proposal.Status = nextStatus;
        proposal.ResolvedAt = nextStatus is AlbumProposalStatuses.Open ? null : DateTime.UtcNow;
    }

    public static void AdmitIfApproved(Album album, AlbumProposal proposal, string nextStatus)
    {
        ApplyStatus(proposal, nextStatus);
        if (nextStatus != AlbumProposalStatuses.Approved ||
            album.Tracks.Any(track => track.SongId == proposal.SongId))
        {
            return;
        }

        var sortOrder = album.Tracks.Count == 0 ? 1 : album.Tracks.Max(track => track.SortOrder) + 1;
        album.Tracks.Add(new AlbumTrack
        {
            AlbumId = album.Id,
            SongId = proposal.SongId,
            Proposal = proposal,
            SortOrder = sortOrder,
            AddedAt = DateTime.UtcNow,
            Song = proposal.Song
        });
    }

    private static string MemberLabel(Member? member)
    {
        if (member is null)
        {
            return "Member";
        }

        return string.IsNullOrWhiteSpace(member.Fullname) ? member.Username : member.Fullname;
    }
}
