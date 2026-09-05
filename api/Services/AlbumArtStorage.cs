using api.Data.Entities;
using api.Integrations.Google;

namespace api.Services;

public static class AlbumArtStorage
{
    public static async Task<bool> ClearIfMissingAsync(
        Album album,
        IGoogleDriveService googleDrive,
        int memberId,
        CancellationToken cancellationToken)
    {
        var appliedId = album.ArtDriveFileId?.Trim();
        if (string.IsNullOrEmpty(appliedId))
        {
            return false;
        }

        var presence = await googleDrive.GetFilePresenceAsync(memberId, appliedId, cancellationToken);
        if (presence != DriveFilePresence.NotFound)
        {
            return false;
        }

        album.ArtDriveFileId = null;
        return true;
    }
}
