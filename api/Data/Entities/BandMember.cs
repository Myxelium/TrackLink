namespace api.Data.Entities
{
    public partial class BandMember
    {
        public int Id { get; set; }

        public int BandId { get; set; }

        public int MemberId { get; set; }

        public virtual Band Band { get; set; } = null!;

        public virtual Member Member { get; set; } = null!;
    }
}
