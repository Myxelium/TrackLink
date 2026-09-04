namespace api.Data.Entities
{
    public partial class Member
    {
        public int Id { get; set; }

        public Guid UserIdentifier { get; set; }

        public string Username { get; set; } = null!;

        public string? Fullname { get; set; }

        public string? Email { get; set; }

        public string? GoogleSubject { get; set; }

        public string? Image { get; set; }

        public virtual ICollection<BandMember> BandMembers { get; set; } = new List<BandMember>();

        public virtual ICollection<MemberRole> MemberRoles { get; set; } = new List<MemberRole>();

        public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

        public virtual ICollection<Song> Songs { get; set; } = new List<Song>();

        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();

        public virtual GoogleAccount? GoogleAccount { get; set; }

        public virtual ICollection<BandInvite> CreatedInvites { get; set; } = new List<BandInvite>();

        public virtual ICollection<Band> OwnedBands { get; set; } = new List<Band>();

        public virtual ICollection<Album> CreatedAlbums { get; set; } = new List<Album>();

        public virtual ICollection<AlbumProposal> AlbumProposals { get; set; } = new List<AlbumProposal>();

        public virtual ICollection<AlbumProposalDecision> AlbumProposalDecisions { get; set; } = new List<AlbumProposalDecision>();

        public virtual ICollection<AlbumProposalReview> AlbumProposalReviews { get; set; } = new List<AlbumProposalReview>();
    }
}
