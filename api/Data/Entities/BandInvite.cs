namespace api.Data.Entities;

public class BandInvite
{
    public int Id { get; set; }

    public int BandId { get; set; }

    public string Email { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string RoleName { get; set; } = "member";

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public virtual Band Band { get; set; } = null!;

    public virtual Member CreatedByNavigation { get; set; } = null!;
}
