namespace api.Data.Entities
{
    public class GoogleAccount
    {
        public int Id { get; set; }

        public string? Email { get; set; }

        public string AccessToken { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        public DateTimeOffset ExpiresAt { get; set; }
    }
}
