using Microsoft.AspNetCore.DataProtection;

namespace api.Services;

public class MemberSession(IDataProtectionProvider protection) : IMemberSession
{
    public const string CookieName = "tracklink.sid";

    private readonly IDataProtector _protector = protection.CreateProtector("TrackLink.MemberSession");

    public int? GetMemberId(HttpContext http)
    {
        if (!http.Request.Cookies.TryGetValue(CookieName, out var cookie) ||
            string.IsNullOrWhiteSpace(cookie))
        {
            return null;
        }

        try
        {
            var raw = _protector.Unprotect(cookie);
            return int.TryParse(raw, out var memberId) ? memberId : null;
        }
        catch
        {
            return null;
        }
    }

    public void SignIn(HttpContext http, int memberId)
    {
        http.Response.Cookies.Append(CookieName, _protector.Protect(memberId.ToString()), new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = http.Request.IsHttps,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });
    }

    public void SignOut(HttpContext http)
    {
        http.Response.Cookies.Delete(CookieName, new CookieOptions { Path = "/" });
    }
}
