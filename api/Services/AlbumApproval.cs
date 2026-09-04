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
