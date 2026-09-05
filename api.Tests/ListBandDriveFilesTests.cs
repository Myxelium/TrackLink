using api.Contracts;
using api.Data;
using api.Data.Entities;
using api.Handlers.Bands;
using api.Integrations.Google;
using api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests;

public class ListBandDriveFilesTests
{
    [Fact]
    public async Task Default_list_returns_audio_and_never_images()
    {
        var fx = await OpenBand();
        var drive = new MixedFolderDrive();

        var listed = await new ListBandDriveFiles.Handler(fx.Db, drive).Handle(
            new ListBandDriveFiles.Query(fx.Band.Id, fx.Owner.Id),
            CancellationToken.None);

        Assert.Null(listed.Error);
        Assert.Equal(["take-night"], listed.Files.Select(file => file.Id).ToArray());
        Assert.All(listed.Files, file => Assert.True(DriveFileKinds.IsAudioMime(file.MimeType)));
        Assert.DoesNotContain(listed.Files, file => DriveFileKinds.IsImageMime(file.MimeType));
        Assert.Equal(1, drive.AudioListCount);
        Assert.Equal(0, drive.ImageListCount);
    }

    [Fact]
    public async Task Image_kind_returns_images_and_never_audio()
    {
        var fx = await OpenBand();
        var drive = new MixedFolderDrive();

        var listed = await new ListBandDriveFiles.Handler(fx.Db, drive).Handle(
            new ListBandDriveFiles.Query(fx.Band.Id, fx.Owner.Id, DriveFileKinds.Image),
            CancellationToken.None);

        Assert.Null(listed.Error);
        Assert.Equal(["cover-dawn"], listed.Files.Select(file => file.Id).ToArray());
        Assert.All(listed.Files, file => Assert.True(DriveFileKinds.IsImageMime(file.MimeType)));
        Assert.DoesNotContain(listed.Files, file => DriveFileKinds.IsAudioMime(file.MimeType));
        Assert.Equal(0, drive.AudioListCount);
        Assert.Equal(1, drive.ImageListCount);
    }

    [Fact]
    public async Task Invalid_kind_is_rejected()
    {
        var fx = await OpenBand();
        var drive = new MixedFolderDrive();

        var listed = await new ListBandDriveFiles.Handler(fx.Db, drive).Handle(
            new ListBandDriveFiles.Query(fx.Band.Id, fx.Owner.Id, "video"),
            CancellationToken.None);

        Assert.Equal("invalid_kind", listed.Error);
        Assert.Empty(listed.Files);
        Assert.Equal(0, drive.AudioListCount);
        Assert.Equal(0, drive.ImageListCount);
    }

    private static async Task<BandFixture> OpenBand()
    {
        var db = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        var owner = new Member { UserIdentifier = Guid.NewGuid(), Username = "ada" };
        var band = new Band
        {
            Name = "Kindred",
            CreatedDate = DateTime.UtcNow,
            DriveFolderId = "folder-root"
        };
        db.Members.Add(owner);
        db.Bands.Add(band);
        await db.SaveChangesAsync();
        band.OwnerMemberId = owner.Id;
        db.BandMembers.Add(new BandMember
        {
            BandId = band.Id,
            MemberId = owner.Id,
            RoleName = BandRoles.Owner
        });
        await db.SaveChangesAsync();
        return new BandFixture(db, owner, band);
    }

    private sealed record BandFixture(DatabaseContext Db, Member Owner, Band Band);

    private sealed class MixedFolderDrive : IGoogleDriveService
    {
        public int AudioListCount { get; private set; }

        public int ImageListCount { get; private set; }

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
            CancellationToken cancellationToken)
        {
            AudioListCount++;
            return Task.FromResult<IReadOnlyList<DriveFileDto>>([
                new DriveFileDto("take-night", "night.mp3", "audio/mpeg")
            ]);
        }

        public Task<IReadOnlyList<DriveFileDto>> ListImageFilesAsync(
            int memberId,
            string folderId,
            CancellationToken cancellationToken)
        {
            ImageListCount++;
            return Task.FromResult<IReadOnlyList<DriveFileDto>>([
                new DriveFileDto("cover-dawn", "dawn.jpg", "image/jpeg")
            ]);
        }

        public Task<bool> CanReadFolderAsync(int memberId, string folderId, CancellationToken cancellationToken) =>
            Task.FromResult(true);

        public Task<bool> IsFileInsideFolderAsync(
            int memberId,
            string fileId,
            string folderId,
            CancellationToken cancellationToken) =>
            Task.FromResult(true);

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
    }
}
