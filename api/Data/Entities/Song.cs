namespace api.Data.Entities
{
    public partial class Song
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public int UploadedBy { get; set; }

        public int? Version { get; set; }

        public int PreviousVersion { get; set; }

        public string? ContentMd5 { get; set; }

        public DateTime? SourceModifiedAt { get; set; }

        public string Url { get; set; } = null!;

        public virtual Member UploadedByNavigation { get; set; } = null!;

        public virtual ICollection<SongIdentifier> SongIdentifiers { get; set; } = new List<SongIdentifier>();

        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();

        public virtual ICollection<AlbumTrack> AlbumTracks { get; set; } = new List<AlbumTrack>();

        public virtual ICollection<AlbumProposal> AlbumProposals { get; set; } = new List<AlbumProposal>();
    }
}
