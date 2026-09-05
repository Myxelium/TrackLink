namespace api.Data.Entities
{
    public partial class Vote
    {
        public int Id { get; set; }

        public int MemberId { get; set; }

        public int? SongId { get; set; }

        public int? AlbumId { get; set; }

        public string? Kind { get; set; }

        public string? Choice { get; set; }

        public string? Subject { get; set; }

        public string? Comment { get; set; }

        public virtual Member Member { get; set; } = null!;

        public virtual Song? Song { get; set; }

        public virtual Album? Album { get; set; }
    }
}