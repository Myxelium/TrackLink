namespace api.Data.Entities
{
    public partial class Band
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Image { get; set; }

        public string? Genre { get; set; }

        public DateTime CreatedDate { get; set; }

        public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    }
}