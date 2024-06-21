namespace api.Data.Entities
{
    public partial class Vote
    {
        public int Id { get; set; }

        public int MemberId { get; set; }

        public int SongId { get; set; }

        public string? Comment { get; set; }

        public virtual Member Member { get; set; } = null!;

        public virtual Song Song { get; set; } = null!;
    }
}