namespace api.Services;

public interface IMemberSession
{
    int? GetMemberId(HttpContext http);

    void SignIn(HttpContext http, int memberId);

    void SignOut(HttpContext http);
}
