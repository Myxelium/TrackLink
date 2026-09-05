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

    public static AlbumDetailDto ToDetail(Album album, IReadOnlyList<BandMember> members, int? memberId = null)
    {
        var tracks = album.Tracks
            .OrderBy(track => track.SortOrder)
            .ThenBy(track => track.SongId)
            .ToList();
        var order = TallyOrder(album, tracks, memberId);

        return new AlbumDetailDto(
            album.Id,
            album.BandId,
            album.Name,
            album.Archived,
            album.ApprovalRule,
            tracks
                .Select(track => new AlbumTrackDto(
                    track.Id,
                    track.SongId,
                    track.Song.Name,
                    track.Song.Version,
                    track.SortOrder,
                    track.AddedAt,
                    TallyInclusion(album.Votes, track.SongId, memberId),
                    TallyNames(album.Votes, AlbumVoteKinds.SongName, track.SongId, track.Song.Name, memberId),
                    order.BySong.GetValueOrDefault(
                        track.SongId,
                        new OrderTrackTallyDto(null, null, null))))
                .ToList(),
            album.Proposals
                .OrderByDescending(proposal => proposal.CreatedAt)
                .Select(proposal => ToProposal(proposal, album.ApprovalRule, members))
                .ToList(),
            TallyNames(album.Votes, AlbumVoteKinds.AlbumName, null, album.Name, memberId),
            order.Contest,
            TallyArt(album, memberId));
    }

    public static (OrderContestDto Contest, Dictionary<int, OrderTrackTallyDto> BySong) TallyOrder(
        Album album,
        IReadOnlyList<AlbumTrack> tracks,
        int? memberId)
    {
        var admittedIds = tracks.Select(track => track.SongId).ToHashSet();
        var orderVotes = album.Votes
            .Where(vote => vote.Kind == AlbumVoteKinds.Order && vote.AlbumId == album.Id)
            .ToList();
        var mySongIds = memberId is int voterId
            ? AlbumOrderVotes.ParseBallot(orderVotes.Where(vote => vote.MemberId == voterId), admittedIds)
            : null;
        var ballots = AlbumOrderVotes.ValidBallots(album);
        IReadOnlyList<int> consensus = ballots.Count == 0
            ? []
            : AlbumOrderVotes.ConsensusSongIds(album);
        var bySong = new Dictionary<int, OrderTrackTallyDto>();

        foreach (var track in tracks)
        {
            var myRank = mySongIds is null ? (int?)null : AlbumOrderVotes.IndexOf(mySongIds, track.SongId) + 1;
            var averageRank = ballots.Count == 0
                ? (double?)null
                : ballots.Average(ballot => AlbumOrderVotes.IndexOf(ballot, track.SongId) + 1.0);
            var consensusPosition = consensus.Count == 0
                ? (int?)null
                : AlbumOrderVotes.IndexOf(consensus, track.SongId) + 1;

            bySong[track.SongId] = new OrderTrackTallyDto(
                myRank is > 0 ? myRank : null,
                averageRank,
                consensusPosition is > 0 ? consensusPosition : null);
        }

        return (new OrderContestDto(album.OrderLocked, ballots.Count, mySongIds), bySong);
    }

    public static ArtContestDto TallyArt(Album album, int? memberId)
    {
        var artVotes = album.Votes
            .Where(vote => vote.Kind == AlbumVoteKinds.Art &&
                           vote.AlbumId == album.Id &&
                           !string.IsNullOrWhiteSpace(vote.Subject))
            .ToList();
        var myDriveFileId = artVotes.FirstOrDefault(vote => vote.MemberId == memberId)?.Subject;
        var grouped = artVotes
            .GroupBy(vote => vote.Subject!, StringComparer.Ordinal)
            .Select(group => new ArtCandidateDto(
                group.Key,
                group.Count(),
                myDriveFileId is not null &&
                string.Equals(myDriveFileId, group.Key, StringComparison.Ordinal)))
            .ToList();

        if (!string.IsNullOrWhiteSpace(album.ArtDriveFileId) &&
            grouped.All(candidate =>
                !string.Equals(candidate.DriveFileId, album.ArtDriveFileId, StringComparison.Ordinal)))
        {
            grouped.Add(new ArtCandidateDto(album.ArtDriveFileId, 0, false));
        }

        return new ArtContestDto(
            album.ArtLocked,
            album.ArtDriveFileId,
            artVotes.Count,
            myDriveFileId,
            grouped
                .OrderByDescending(candidate => candidate.VoteCount)
                .ThenBy(candidate => candidate.DriveFileId, StringComparer.Ordinal)
                .ToList());
    }

    public static InclusionTallyDto TallyInclusion(
        IEnumerable<Vote> votes,
        int songId,
        int? memberId)
    {
        var forSong = votes
            .Where(vote => vote.AlbumId is not null &&
                           vote.Kind == AlbumVoteKinds.Inclusion &&
                           vote.SongId == songId &&
                           AlbumInclusionChoices.IsKnown(vote.Choice))
            .ToList();

        return new InclusionTallyDto(
            forSong.FirstOrDefault(vote => vote.MemberId == memberId)?.Choice,
            forSong.Count(vote => vote.Choice == AlbumInclusionChoices.In),
            forSong.Count(vote => vote.Choice == AlbumInclusionChoices.Out),
            forSong.Count(vote => vote.Choice == AlbumInclusionChoices.Abstain));
    }

    public static NameContestDto TallyNames(
        IEnumerable<Vote> votes,
        string kind,
        int? songId,
        string officialName,
        int? memberId)
    {
        var forContest = votes
            .Where(vote => vote.AlbumId is not null &&
                           vote.Kind == kind &&
                           !string.IsNullOrWhiteSpace(vote.Subject) &&
                           (kind == AlbumVoteKinds.AlbumName
                               ? vote.SongId is null
                               : vote.SongId == songId))
            .ToList();

        var myName = forContest.FirstOrDefault(vote => vote.MemberId == memberId)?.Subject;
        var grouped = forContest
            .GroupBy(vote => vote.Subject!, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var displayName = AlbumNameVotes.CanonicalName(
                    group.Select(vote => vote.Subject!).Prepend(officialName),
                    group.Key);
                return new NameCandidateDto(
                    displayName,
                    group.Count(),
                    myName is not null &&
                    string.Equals(myName, group.Key, StringComparison.OrdinalIgnoreCase));
            })
            .ToList();

        if (!string.IsNullOrWhiteSpace(officialName) &&
            grouped.All(candidate =>
                !string.Equals(candidate.Name, officialName, StringComparison.OrdinalIgnoreCase)))
        {
            grouped.Add(new NameCandidateDto(officialName, 0, false));
        }

        return new NameContestDto(
            myName,
            grouped
                .OrderByDescending(candidate => candidate.VoteCount)
                .ThenBy(candidate => candidate.Name, StringComparer.OrdinalIgnoreCase)
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
