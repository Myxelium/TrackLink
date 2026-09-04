namespace api.Data.Entities;

public class AlbumProposal
{
    public int Id { get; set; }

    public int AlbumId { get; set; }

    public int SongId { get; set; }

    public int ProposedBy { get; set; }

    public string Status { get; set; } = "open";

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public virtual Album Album { get; set; } = null!;

    public virtual Song Song { get; set; } = null!;

    public virtual Member ProposedByNavigation { get; set; } = null!;

    public virtual ICollection<AlbumProposalDecision> Decisions { get; set; } = new List<AlbumProposalDecision>();

    public virtual ICollection<AlbumProposalReview> Reviews { get; set; } = new List<AlbumProposalReview>();
}
