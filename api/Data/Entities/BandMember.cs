namespace api.Data.Entities
{
    public partial class BandMember
    {
        public int Id { get; set; }

        public int BandId { get; set; }

        public int MemberId { get; set; }

        public virtual Band Member { get; set; } = null!;

        public virtual Member MemberNavigation { get; set; } = null!;
    }
}