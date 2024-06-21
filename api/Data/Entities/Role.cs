namespace api.Data.Entities
{
    public partial class Role
    {
        public int Id { get; set; }

        public int CreatedBy { get; set; }

        public int CreatedFor { get; set; }

        public string RoleName { get; set; } = null!;

        public string? RoleDescription { get; set; }

        public DateTime? CreatedDate { get; set; }

        public virtual Member CreatedByNavigation { get; set; } = null!;

        public virtual Band CreatedForNavigation { get; set; } = null!;
    }
}