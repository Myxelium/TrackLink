using api.Data.Entities;

namespace api.Services;

public static class AlbumApproval
{
    public const string All = "all";
    public const string OwnerUploaders = "owner_uploaders";

    public static bool IsKnown(string? ruleName)
    {
        return ruleName is All or OwnerUploaders;
    }

    public static string Normalize(string? ruleName)
    {
        return IsKnown(ruleName) ? ruleName! : All;
    }

    public static bool CountsTowardRule(string ruleName, string? roleName)
    {
        return ruleName == OwnerUploaders
            ? BandRoles.CanUpload(roleName)
            : BandRoles.IsKnown(roleName);
    }

    public static IReadOnlyList<BandMember> RequiredMembers(
        string ruleName,
        IReadOnlyList<BandMember> members)
    {
        var whoCanVote = members.Where(member => CanCastAVote(member.Member)).ToList();
        var pool = whoCanVote.Count > 0 ? whoCanVote : members;

        return pool.Where(member => CountsTowardRule(ruleName, member.RoleName)).ToList();
    }

    private static bool CanCastAVote(Member? member)
    {
        return member is not null &&
            (!string.IsNullOrWhiteSpace(member.Email) ||
             !string.IsNullOrWhiteSpace(member.GoogleSubject) ||
             member.GoogleAccount is not null);
    }
}

public static class AlbumProposalStatuses
{
    public const string Open = "open";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
    public const string Withdrawn = "withdrawn";
}

public static class AlbumProposalDecisions
{
    public const string Approve = "approve";
    public const string Reject = "reject";

    public static bool IsKnown(string? decision)
    {
        return decision is Approve or Reject;
    }
}

public static class AlbumInclusionChoices
{
    public const string In = "in";
    public const string Out = "out";
    public const string Abstain = "abstain";

    public static bool IsKnown(string? choice)
    {
        return choice is In or Out or Abstain;
    }
}

public static class AlbumVoteKinds
{
    public const string Inclusion = "inclusion";
    public const string AlbumName = "album_name";
    public const string SongName = "song_name";
    public const string Order = "order";
    public const string Art = "art";
}

public static class AlbumArtVotes
{
    public const int MaxLength = 128;

    public static string? Normalize(string? rawFileId)
    {
        var fileId = rawFileId?.Trim();
        if (string.IsNullOrEmpty(fileId) || fileId.Length > MaxLength)
        {
            return null;
        }

        return fileId.All(character => char.IsLetterOrDigit(character) || character is '_' or '-')
            ? fileId
            : null;
    }

    public static string? ConsensusDriveFileId(Album album)
    {
        var tallies = album.Votes
            .Where(vote => vote.Kind == AlbumVoteKinds.Art && !string.IsNullOrWhiteSpace(vote.Subject))
            .GroupBy(vote => vote.Subject!, StringComparer.Ordinal)
            .Select(group => (DriveFileId: group.Key, VoteCount: group.Count()))
            .ToList();
        if (tallies.Count == 0)
        {
            return album.ArtDriveFileId;
        }

        return tallies
            .OrderByDescending(tally => tally.VoteCount)
            .ThenByDescending(tally =>
                string.Equals(tally.DriveFileId, album.ArtDriveFileId, StringComparison.Ordinal))
            .ThenBy(tally => tally.DriveFileId, StringComparer.Ordinal)
            .First()
            .DriveFileId;
    }

    public static void ApplyConsensusArt(Album album)
    {
        album.ArtDriveFileId = ConsensusDriveFileId(album);
    }
}

public static class AlbumNameVotes
{
    public const int MaxLength = 50;

    public static string? Normalize(string? rawName)
    {
        var name = rawName?.Trim();
        if (string.IsNullOrEmpty(name) || name.Length > MaxLength)
        {
            return null;
        }

        return name;
    }

    public static string CanonicalName(IEnumerable<string> existingNames, string name)
    {
        return existingNames.FirstOrDefault(existing =>
                   string.Equals(existing, name, StringComparison.OrdinalIgnoreCase))
               ?? name;
    }
}

public static class AlbumOrderVotes
{
    public static bool TryParseRank(string? choice, out int rank)
    {
        return int.TryParse(choice, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out rank)
               && rank > 0;
    }

    public static string RankChoice(int rank)
    {
        return rank.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    public static bool IsCompletePermutation(IReadOnlyList<int> songIds, IReadOnlyCollection<int> admittedIds)
    {
        return songIds.Count == admittedIds.Count &&
               songIds.Distinct().Count() == songIds.Count &&
               songIds.All(admittedIds.Contains);
    }

    public static IReadOnlyList<int>? ParseBallot(IEnumerable<Vote> votes, IReadOnlySet<int> admittedIds)
    {
        var ranks = new List<(int SongId, int Rank)>();
        foreach (var vote in votes)
        {
            if (vote.SongId is null || !TryParseRank(vote.Choice, out var rank))
            {
                return null;
            }

            ranks.Add((vote.SongId.Value, rank));
        }

        if (ranks.Count != admittedIds.Count ||
            ranks.Select(entry => entry.SongId).ToHashSet().SetEquals(admittedIds) is false)
        {
            return null;
        }

        var expectedRanks = Enumerable.Range(1, admittedIds.Count);
        if (ranks.Select(entry => entry.Rank).OrderBy(rank => rank).SequenceEqual(expectedRanks) is false)
        {
            return null;
        }

        return ranks.OrderBy(entry => entry.Rank).Select(entry => entry.SongId).ToList();
    }

    public static IReadOnlyList<IReadOnlyList<int>> ValidBallots(Album album)
    {
        var admittedIds = album.Tracks.Select(track => track.SongId).ToHashSet();
        if (admittedIds.Count == 0)
        {
            return [];
        }

        return album.Votes
            .Where(vote => vote.Kind == AlbumVoteKinds.Order && vote.AlbumId == album.Id)
            .GroupBy(vote => vote.MemberId)
            .Select(group => ParseBallot(group, admittedIds))
            .OfType<IReadOnlyList<int>>()
            .ToList();
    }

    public static IReadOnlyList<int> ConsensusSongIds(Album album)
    {
        var tracks = album.Tracks
            .OrderBy(track => track.SortOrder)
            .ThenBy(track => track.SongId)
            .ToList();
        var ballots = ValidBallots(album);
        if (ballots.Count == 0)
        {
            return tracks.Select(track => track.SongId).ToList();
        }

        return tracks
            .OrderBy(track => ballots.Average(ballot => IndexOf(ballot, track.SongId) + 1.0))
            .ThenBy(track => track.SortOrder)
            .ThenBy(track => track.SongId)
            .Select(track => track.SongId)
            .ToList();
    }

    public static void ApplyConsensusOrder(Album album)
    {
        var consensus = ConsensusSongIds(album);
        for (var index = 0; index < consensus.Count; index++)
        {
            var track = album.Tracks.First(item => item.SongId == consensus[index]);
            track.SortOrder = index + 1;
        }
    }

    public static int IndexOf(IReadOnlyList<int> songIds, int songId)
    {
        for (var index = 0; index < songIds.Count; index++)
        {
            if (songIds[index] == songId)
            {
                return index;
            }
        }

        return -1;
    }
}
