namespace api.Data.Entities
{
    public partial class MemberRole
    {
        public int Id { get; set; }

        public int MemberId { get; set; }

        public int RoleId { get; set; }

        public virtual ICollection<MemberRole> InverseRole { get; set; } = new List<MemberRole>();

        public virtual Member Member { get; set; } = null!;

        public virtual MemberRole Role { get; set; } = null!;
    }
}