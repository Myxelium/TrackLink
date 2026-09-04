namespace api.Data.Entities
{
    public partial class Band
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Image { get; set; }

        public string? Genre { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? OwnerMemberId { get; set; }

        public string? DriveFolderId { get; set; }

        public string? DriveFolderName { get; set; }

        public virtual Member? OwnerMember { get; set; }

        public virtual ICollection<BandMember> BandMembers { get; set; } = new List<BandMember>();

        public virtual ICollection<BandInvite> BandInvites { get; set; } = new List<BandInvite>();

        public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

        public virtual ICollection<SongIdentifier> SongIdentifiers { get; set; } = new List<SongIdentifier>();

        public virtual ICollection<Album> Albums { get; set; } = new List<Album>();
    }
}
