namespace api.Data.Entities
{
    public partial class SongIdentifier
    {
        public int Id { get; set; }

        public int BandId { get; set; }

        public int SongId { get; set; }

        public virtual Band Band { get; set; } = null!;

        public virtual Song Song { get; set; } = null!;
    }
}
