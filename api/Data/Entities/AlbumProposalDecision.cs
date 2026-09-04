namespace api.Data.Entities;

public class AlbumProposalDecision
{
    public int Id { get; set; }

    public int ProposalId { get; set; }

    public int MemberId { get; set; }

    public string Decision { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }

    public virtual AlbumProposal Proposal { get; set; } = null!;

    public virtual Member Member { get; set; } = null!;
}
