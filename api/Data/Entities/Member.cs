namespace api.Data.Entities
{
    public partial class Member
    {
        public int Id { get; set; }

        public Guid UserIdentifier { get; set; }

        public string Username { get; set; } = null!;

        public string? Fullname { get; set; }

        public string? Image { get; set; }

        public virtual ICollection<MemberRole> MemberRoles { get; set; } = new List<MemberRole>();

        public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

        public virtual ICollection<Song> Songs { get; set; } = new List<Song>();

        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}