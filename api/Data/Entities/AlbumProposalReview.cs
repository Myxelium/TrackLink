namespace api.Data.Entities;

public class AlbumProposalReview
{
    public int Id { get; set; }

    public int ProposalId { get; set; }

    public int MemberId { get; set; }

    public int StartMs { get; set; }

    public int EndMs { get; set; }

    public string Body { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual AlbumProposal Proposal { get; set; } = null!;

    public virtual Member Member { get; set; } = null!;
}
