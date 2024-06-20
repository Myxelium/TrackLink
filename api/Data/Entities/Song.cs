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

        public string Url { get; set; } = null!;

        public virtual Member UploadedByNavigation { get; set; } = null!;

        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}