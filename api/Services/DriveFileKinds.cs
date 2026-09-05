namespace api.Services;

public static class DriveFileKinds
{
    public const string Audio = "audio";
    public const string Image = "image";

    public static string? Normalize(string? kind)
    {
        if (string.IsNullOrWhiteSpace(kind))
        {
            return Audio;
        }

        var trimmed = kind.Trim().ToLowerInvariant();
        return trimmed is Audio or Image ? trimmed : null;
    }

    public static bool IsAudioMime(string? mimeType)
    {
        return mimeType is not null && mimeType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsImageMime(string? mimeType)
    {
        return mimeType is not null && mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }
}
