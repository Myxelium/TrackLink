using api.Contracts;
using api.Data;
using api.Data.Entities;
using api.Handlers.Albums;
using api.Integrations.Google;
using api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace api.Tests;

public class UploadAlbumArtTests
{
    [Fact]
    public async Task Uploads_an_image_into_the_band_folder_without_applying_the_cover()
    {
        var fx = await OpenBand();
        var drive = new RecordingDrive("folder-root", "cover-tiny");

        var uploaded = await ArtHandler(fx.Db, drive).Handle(
            new UploadAlbumArt.Command(
                fx.Album.Id,
                fx.Owner.Id,
                "tiny.png",
                "image/png",
                new MemoryStream(AlbumArtPixelsTests.PngHeader(400, 400)),
                24),
            CancellationToken.None);

        Assert.Null(uploaded.Error);
        Assert.Equal("cover-tiny", uploaded.Value!.DriveFileId);
        Assert.Equal("tiny.png", uploaded.Value.Name);
        Assert.Equal("image/png", uploaded.Value.MimeType);
        Assert.Equal(400, uploaded.Value.Width);
        Assert.Equal(400, uploaded.Value.Height);
        Assert.Equal(
            "Cover is 400x400. Recommended size is 1600-3000 pixels on each side.",
            uploaded.Value.Warning);
        Assert.Equal(["folder-root"], drive.UploadedFolderIds);
        Assert.Null(await fx.Db.Albums.Where(item => item.Id == fx.Album.Id)
            .Select(item => item.ArtDriveFileId)
            .SingleAsync());
        Assert.Empty(await fx.Db.Votes.Where(vote => vote.Kind == AlbumVoteKinds.Art).ToListAsync());
    }

    [Fact]
    public async Task Listener_can_upload_a_recommended_cover_without_a_warning()
    {
        var fx = await OpenBand();
        var drive = new RecordingDrive("folder-root", "cover-dawn");

        var uploaded = await ArtHandler(fx.Db, drive).Handle(
            new UploadAlbumArt.Command(
                fx.Album.Id,
                fx.Listener.Id,
                "dawn.png",
                "image/png",
                new MemoryStream(AlbumArtPixelsTests.PngHeader(2000, 2000)),
                24),
            CancellationToken.None);

        Assert.Null(uploaded.Error);
        Assert.Null(uploaded.Value!.Warning);
        Assert.Equal(2000, uploaded.Value.Width);
        Assert.Equal(["folder-root"], drive.UploadedFolderIds);
    }

    [Fact]
    public async Task Rejects_an_upload_that_lands_outside_the_band_folder()
    {
        var fx = await OpenBand();
        var drive = new RecordingDrive("other-folder", "cover-outside");

        var uploaded = await ArtHandler(fx.Db, drive).Handle(
            new UploadAlbumArt.Command(
                fx.Album.Id,
                fx.Owner.Id,
                "away.png",
                "image/png",
                new MemoryStream(AlbumArtPixelsTests.PngHeader(2000, 2000)),
                24),
            CancellationToken.None);

        Assert.Equal("outside_folder", uploaded.Error);
        Assert.Null(await fx.Db.Albums.Where(item => item.Id == fx.Album.Id)
            .Select(item => item.ArtDriveFileId)
            .SingleAsync());
    }

    [Fact]
    public async Task Rejects_a_non_image()
    {
        var fx = await OpenBand();
        var drive = new RecordingDrive("folder-root", "not-art");

        var uploaded = await ArtHandler(fx.Db, drive).Handle(
            new UploadAlbumArt.Command(
                fx.Album.Id,
                fx.Owner.Id,
                "take.mp3",
                "audio/mpeg",
                new MemoryStream([1, 2, 3, 4]),
                4),
            CancellationToken.None);

        Assert.Equal("not_image", uploaded.Error);
        Assert.Empty(drive.UploadedFolderIds);
    }

    [Fact]
    public async Task Rejects_a_file_over_the_size_cap()
    {
        var fx = await OpenBand();
        var drive = new RecordingDrive("folder-root", "cover-huge");

        var uploaded = await ArtHandler(fx.Db, drive).Handle(
            new UploadAlbumArt.Command(
                fx.Album.Id,
                fx.Owner.Id,
                "huge.png",
                "image/png",
                new MemoryStream(AlbumArtPixelsTests.PngHeader(2000, 2000)),
                AlbumArtPixels.MaxBytes + 1),
            CancellationToken.None);

        Assert.Equal("too_large", uploaded.Error);
        Assert.Empty(drive.UploadedFolderIds);
    }

    [Fact]
    public async Task Locked_art_rejects_uploads()
    {
        var fx = await OpenBand();
        fx.Album.ArtLocked = true;
        await fx.Db.SaveChangesAsync();
        var drive = new RecordingDrive("folder-root", "cover-late");

        var uploaded = await ArtHandler(fx.Db, drive).Handle(
            new UploadAlbumArt.Command(
                fx.Album.Id,
                fx.Owner.Id,
                "late.png",
                "image/png",
                new MemoryStream(AlbumArtPixelsTests.PngHeader(2000, 2000)),
                24),
            CancellationToken.None);

        Assert.Equal("art_locked", uploaded.Error);
        Assert.Empty(drive.UploadedFolderIds);
    }

    [Fact]
    public async Task Drive_write_denied_is_a_reauth_error_not_an_exception()
    {
        var fx = await OpenBand();
        var drive = new RecordingDrive("folder-root", "cover-denied", new DriveWriteDeniedException());

        var uploaded = await ArtHandler(fx.Db, drive).Handle(
            new UploadAlbumArt.Command(
                fx.Album.Id,
                fx.Owner.Id,
                "dawn.png",
                "image/png",
                new MemoryStream(AlbumArtPixelsTests.PngHeader(2000, 2000)),
                24),
            CancellationToken.None);

        Assert.Equal("needs_reauth", uploaded.Error);
        Assert.Empty(drive.UploadedFolderIds);
    }

    [Fact]
    public async Task Unexpected_drive_exception_is_an_upload_error_not_an_exception()
    {
        var fx = await OpenBand();
        var drive = new RecordingDrive("folder-root", "cover-boom", new InvalidOperationException("Drive exploded"));

        var uploaded = await ArtHandler(fx.Db, drive).Handle(
            new UploadAlbumArt.Command(
                fx.Album.Id,
                fx.Owner.Id,
                "dawn.png",
                "image/png",
                new MemoryStream(AlbumArtPixelsTests.PngHeader(2000, 2000)),
                24),
            CancellationToken.None);

        Assert.Equal("upload_failed", uploaded.Error);
        Assert.Empty(drive.UploadedFolderIds);
    }

    [Fact]
    public async Task Caps_a_stream_that_is_larger_than_the_declared_length()
    {
        var fx = await OpenBand();
        var drive = new RecordingDrive("folder-root", "cover-huge");
        var oversized = new byte[AlbumArtPixels.MaxBytes + 1];
        AlbumArtPixelsTests.PngHeader(2000, 2000).CopyTo(oversized, 0);

        var uploaded = await ArtHandler(fx.Db, drive).Handle(
            new UploadAlbumArt.Command(
                fx.Album.Id,
                fx.Owner.Id,
                "huge.png",
                "image/png",
                new MemoryStream(oversized),
                24),
            CancellationToken.None);

        Assert.Equal("too_large", uploaded.Error);
        Assert.Empty(drive.UploadedFolderIds);
    }

    private static UploadAlbumArt.Handler ArtHandler(DatabaseContext db, IGoogleDriveService drive) =>
        new(db, drive, NullLogger<UploadAlbumArt.Handler>.Instance);

    private static async Task<BandFixture> OpenBand()
    {
        var db = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        var owner = new Member { UserIdentifier = Guid.NewGuid(), Username = "ada" };
        var listener = new Member { UserIdentifier = Guid.NewGuid(), Username = "moe" };
        var band = new Band
        {
            Name = "Kindred",
            CreatedDate = DateTime.UtcNow,
            DriveFolderId = "folder-root"
        };
        db.Members.AddRange(owner, listener);
        db.Bands.Add(band);
        await db.SaveChangesAsync();
        band.OwnerMemberId = owner.Id;
        db.BandMembers.AddRange(
            new BandMember { BandId = band.Id, MemberId = owner.Id, RoleName = BandRoles.Owner },
            new BandMember { BandId = band.Id, MemberId = listener.Id, RoleName = BandRoles.Member });
        var album = new Album
        {
            BandId = band.Id,
            Name = "First Light",
            ApprovalRule = AlbumApproval.All,
            CreatedBy = owner.Id,
            CreatedDate = DateTime.UtcNow
        };
        db.Albums.Add(album);
        await db.SaveChangesAsync();
        return new BandFixture(db, owner, listener, album);
    }

    private sealed record BandFixture(DatabaseContext Db, Member Owner, Member Listener, Album Album);

    private sealed class RecordingDrive(
        string acceptedFolderId,
        string createdFileId,
        Exception? uploadError = null) : IGoogleDriveService
    {
        public List<string> UploadedFolderIds { get; } = [];

        public bool IsConfigured => true;

        public Task<string?> CreateAuthorizationUrlAsync(string? state, CancellationToken cancellationToken) =>
            Task.FromResult<string?>(null);

        public Task<GoogleLoginProfile?> ExchangeCodeAsync(string code, CancellationToken cancellationToken) =>
            Task.FromResult<GoogleLoginProfile?>(null);

        public Task SaveTokensAsync(
            int memberId,
            GoogleLoginProfile profile,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<bool> HasTokensAsync(int memberId, CancellationToken cancellationToken) =>
            Task.FromResult(true);

        public Task<string?> GetEmailAsync(int memberId, CancellationToken cancellationToken) =>
            Task.FromResult<string?>(null);

        public Task<string?> GetAccessTokenAsync(int memberId, CancellationToken cancellationToken) =>
            Task.FromResult<string?>(null);

        public Task<IReadOnlyList<DriveFileDto>> ListAudioFilesAsync(
            int memberId,
            string folderId,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<DriveFileDto>>([]);

        public Task<IReadOnlyList<DriveFileDto>> ListImageFilesAsync(
            int memberId,
            string folderId,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<DriveFileDto>>([]);

        public Task<bool> CanReadFolderAsync(int memberId, string folderId, CancellationToken cancellationToken) =>
            Task.FromResult(true);

        public Task<bool> IsFileInsideFolderAsync(
            int memberId,
            string fileId,
            string folderId,
            CancellationToken cancellationToken) =>
            Task.FromResult(folderId == acceptedFolderId && fileId == createdFileId);

        public Task<DriveFileDto?> GetAudioFileAsync(
            int memberId,
            string fileId,
            CancellationToken cancellationToken) =>
            Task.FromResult<DriveFileDto?>(null);

        public Task<DriveDownload?> DownloadAsync(
            int memberId,
            string fileId,
            CancellationToken cancellationToken) =>
            Task.FromResult<DriveDownload?>(null);

        public Task<DriveFileDto?> UploadFileAsync(
            int memberId,
            string folderId,
            string fileName,
            string mimeType,
            Stream content,
            CancellationToken cancellationToken)
        {
            if (uploadError is not null)
            {
                throw uploadError;
            }

            UploadedFolderIds.Add(folderId);
            return Task.FromResult<DriveFileDto?>(new DriveFileDto(createdFileId, fileName, mimeType));
        }
    }
}
