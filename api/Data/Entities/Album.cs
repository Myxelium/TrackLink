namespace api.Data.Entities;

public class Album
{
    public int Id { get; set; }

    public int BandId { get; set; }

    public string Name { get; set; } = null!;

    public bool Archived { get; set; }

    public bool OrderLocked { get; set; }

    public bool ArtLocked { get; set; }

    public string ApprovalRule { get; set; } = "all";

    public string? ArtDriveFileId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Band Band { get; set; } = null!;

    public virtual Member CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<AlbumTrack> Tracks { get; set; } = new List<AlbumTrack>();

    public virtual ICollection<AlbumProposal> Proposals { get; set; } = new List<AlbumProposal>();

    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
