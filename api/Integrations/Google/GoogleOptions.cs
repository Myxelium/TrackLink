namespace api.Integrations.Google;

public class GoogleOptions
{
    public const string SectionName = "Google";

    public string ClientId { get; set; } = "";

    public string ClientSecret { get; set; } = "";

    public string RedirectUri { get; set; } = "http://localhost:5180/api/auth/google/callback";

    public string AppReturnUrl { get; set; } = "http://localhost:4200/";
}
