namespace api.Data.Entities;

public class AlbumTrack
{
    public int Id { get; set; }

    public int AlbumId { get; set; }

    public int SongId { get; set; }

    public int? ProposalId { get; set; }

    public int SortOrder { get; set; }

    public DateTime AddedAt { get; set; }

    public virtual Album Album { get; set; } = null!;

    public virtual Song Song { get; set; } = null!;

    public virtual AlbumProposal? Proposal { get; set; }
}
