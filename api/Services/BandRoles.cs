namespace api.Services;

public static class BandRoles
{
    public const string Owner = "owner";
    public const string Uploader = "uploader";
    public const string Member = "member";

    public static bool IsKnown(string? roleName)
    {
        return roleName is Owner or Uploader or Member;
    }

    public static bool CanUpload(string? roleName)
    {
        return roleName is Owner or Uploader;
    }

    public static bool CanInvite(string? roleName)
    {
        return roleName is Owner;
    }

    public static bool CanSetFolder(string? roleName)
    {
        return roleName is Owner;
    }

    public static bool CanManageAlbums(string? roleName)
    {
        return CanUpload(roleName);
    }
}
