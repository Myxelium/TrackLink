using api.Contracts;
using api.Data;
using api.Data.Entities;
using api.Handlers.Bands;
using api.Integrations.Google;
using api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests;

public class AddBandDriveSongTests
{
    [Fact]
    public async Task First_link_is_version_one_and_keeps_drive_md5()
    {
        var fx = await OpenBand();
        var drive = new FolderDrive(["file-v1"], "abc123", new DateTime(2026, 9, 5, 8, 0, 0, DateTimeKind.Utc));

        var linked = await new AddBandDriveSong.Handler(fx.Db, drive).Handle(
            new AddBandDriveSong.Command(fx.Band.Id, fx.Owner.Id, "file-v1", "bottleneck.mp3"),
            CancellationToken.None);

        Assert.Null(linked.Error);
        Assert.Equal(1, linked.Song!.Version);
        Assert.Equal(0, linked.Song.PreviousVersion);
        Assert.Equal("abc123", linked.Song.ContentMd5);
        Assert.Equal(new DateTime(2026, 9, 5, 8, 0, 0, DateTimeKind.Utc), linked.Song.SourceModifiedAt);
    }

    [Fact]
    public async Task Same_drive_file_returns_the_existing_take()
    {
        var fx = await OpenBand();
        var drive = new FolderDrive(["file-v1"]);
        var first = await new AddBandDriveSong.Handler(fx.Db, drive).Handle(
            new AddBandDriveSong.Command(fx.Band.Id, fx.Owner.Id, "file-v1", "bottleneck.mp3"),
            CancellationToken.None);

        var again = await new AddBandDriveSong.Handler(fx.Db, drive).Handle(
            new AddBandDriveSong.Command(fx.Band.Id, fx.Owner.Id, "file-v1", "bottleneck.mp3"),
            CancellationToken.None);

        Assert.Equal(first.Song!.Id, again.Song!.Id);
        Assert.Equal(1, await fx.Db.Songs.CountAsync());
    }

    [Fact]
    public async Task Same_name_new_file_becomes_the_next_version()
    {
        var fx = await OpenBand();
        var drive = new FolderDrive(["file-v1", "file-v2"]);
        var first = await new AddBandDriveSong.Handler(fx.Db, drive).Handle(
            new AddBandDriveSong.Command(fx.Band.Id, fx.Owner.Id, "file-v1", "bottleneck.mp3"),
            CancellationToken.None);

        var second = await new AddBandDriveSong.Handler(fx.Db, drive).Handle(
            new AddBandDriveSong.Command(fx.Band.Id, fx.Owner.Id, "file-v2", "bottleneck.mp3"),
            CancellationToken.None);

        Assert.Null(second.Error);
        Assert.Equal(2, second.Song!.Version);
        Assert.Equal(first.Song!.Id, second.Song.PreviousVersion);
        Assert.Equal(2, await fx.Db.Songs.CountAsync());
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

    private sealed class FolderDrive(
        IReadOnlyCollection<string> insideFileIds,
        string? md5 = null,
        DateTime? modifiedAt = null) : IGoogleDriveService
    {
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
            Task.FromResult<IReadOnlyList<DriveFileDto>>(
                insideFileIds.Select(fileId => new DriveFileDto(fileId, fileId, "audio/mpeg")).ToList());

        public Task<bool> CanReadFolderAsync(int memberId, string folderId, CancellationToken cancellationToken) =>
            Task.FromResult(true);

        public Task<bool> IsFileInsideFolderAsync(
            int memberId,
            string fileId,
            string folderId,
            CancellationToken cancellationToken) =>
            Task.FromResult(insideFileIds.Contains(fileId));

        public Task<DriveFileDto?> GetAudioFileAsync(
            int memberId,
            string fileId,
            CancellationToken cancellationToken) =>
            Task.FromResult<DriveFileDto?>(
                insideFileIds.Contains(fileId)
                    ? new DriveFileDto(fileId, fileId, "audio/mpeg", md5, modifiedAt)
                    : null);

        public Task<DriveDownload?> DownloadAsync(
            int memberId,
            string fileId,
            CancellationToken cancellationToken) =>
            Task.FromResult<DriveDownload?>(null);
    }
}
