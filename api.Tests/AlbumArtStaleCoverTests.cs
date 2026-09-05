using api.Data;
using api.Data.Entities;
using api.Handlers.Albums;
using api.Integrations.Google;
using api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests;

public class AlbumArtStaleCoverTests
{
    [Fact]
    public async Task Get_album_clears_an_applied_cover_that_is_gone_from_Drive()
    {
        var fx = await OpenBand("cover-gone");
        var drive = new PresenceDrive(DriveFilePresence.NotFound);

        var fetched = await new GetAlbum.Handler(fx.Db, drive).Handle(
            new GetAlbum.Query(fx.Album.Id, fx.Owner.Id),
            CancellationToken.None);

        Assert.Null(fetched.Error);
        Assert.Null(fetched.Value!.Art.AppliedDriveFileId);
        Assert.Null(await fx.Db.Albums.Where(item => item.Id == fx.Album.Id)
            .Select(item => item.ArtDriveFileId)
            .SingleAsync());
        Assert.Equal(["cover-gone"], drive.CheckedFileIds);
    }

    [Fact]
    public async Task Get_album_keeps_the_cover_when_Drive_is_only_temporarily_unavailable()
    {
        var fx = await OpenBand("cover-dawn");
        var drive = new PresenceDrive(DriveFilePresence.Unknown);

        var fetched = await new GetAlbum.Handler(fx.Db, drive).Handle(
            new GetAlbum.Query(fx.Album.Id, fx.Owner.Id),
            CancellationToken.None);

        Assert.Equal("cover-dawn", fetched.Value!.Art.AppliedDriveFileId);
        Assert.Equal("cover-dawn", await fx.Db.Albums.Where(item => item.Id == fx.Album.Id)
            .Select(item => item.ArtDriveFileId)
            .SingleAsync());
    }

    [Fact]
    public async Task Art_preview_clears_a_missing_applied_cover_instead_of_keeping_a_dead_id()
    {
        var fx = await OpenBand("cover-gone");
        var drive = new PresenceDrive(DriveFilePresence.NotFound);

        var opened = await new GetAlbumArt.Handler(fx.Db, drive).Handle(
            new GetAlbumArt.Query(fx.Album.Id, fx.Owner.Id),
            CancellationToken.None);

        Assert.Equal("art_missing", opened.Error);
        Assert.Null(opened.Stream);
        Assert.Null(await fx.Db.Albums.Where(item => item.Id == fx.Album.Id)
            .Select(item => item.ArtDriveFileId)
            .SingleAsync());
        Assert.Equal(0, drive.DownloadCount);
    }

    [Fact]
    public async Task Candidate_preview_does_not_clear_the_applied_cover()
    {
        var fx = await OpenBand("cover-dawn");
        var drive = new PresenceDrive(DriveFilePresence.NotFound);

        var opened = await new GetAlbumArt.Handler(fx.Db, drive).Handle(
            new GetAlbumArt.Query(fx.Album.Id, fx.Owner.Id, "cover-gone"),
            CancellationToken.None);

        Assert.Equal("outside_folder", opened.Error);
        Assert.Equal("cover-dawn", await fx.Db.Albums.Where(item => item.Id == fx.Album.Id)
            .Select(item => item.ArtDriveFileId)
            .SingleAsync());
    }

    private static async Task<BandFixture> OpenBand(string? artDriveFileId)
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
        var album = new Album
        {
            BandId = band.Id,
            Name = "First Light",
            ApprovalRule = AlbumApproval.All,
            ArtDriveFileId = artDriveFileId,
            CreatedBy = owner.Id,
            CreatedDate = DateTime.UtcNow
        };
        db.Albums.Add(album);
        await db.SaveChangesAsync();
        return new BandFixture(db, owner, album);
    }

    private sealed record BandFixture(DatabaseContext Db, Member Owner, Album Album);

    private sealed class PresenceDrive(DriveFilePresence presence) : IGoogleDriveService
    {
        public List<string> CheckedFileIds { get; } = [];

        public int DownloadCount { get; private set; }

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

        public Task<IReadOnlyList<api.Contracts.DriveFileDto>> ListAudioFilesAsync(
            int memberId,
            string folderId,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<api.Contracts.DriveFileDto>>([]);

        public Task<IReadOnlyList<api.Contracts.DriveFileDto>> ListImageFilesAsync(
            int memberId,
            string folderId,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<api.Contracts.DriveFileDto>>([]);

        public Task<bool> CanReadFolderAsync(int memberId, string folderId, CancellationToken cancellationToken) =>
            Task.FromResult(true);

        public Task<bool> IsFileInsideFolderAsync(
            int memberId,
            string fileId,
            string folderId,
            CancellationToken cancellationToken) =>
            Task.FromResult(false);

        public Task<api.Contracts.DriveFileDto?> GetAudioFileAsync(
            int memberId,
            string fileId,
            CancellationToken cancellationToken) =>
            Task.FromResult<api.Contracts.DriveFileDto?>(null);

        public Task<DriveDownload?> DownloadAsync(
            int memberId,
            string fileId,
            CancellationToken cancellationToken)
        {
            DownloadCount++;
            return Task.FromResult<DriveDownload?>(null);
        }

        public Task<DriveFilePresence> GetFilePresenceAsync(
            int memberId,
            string fileId,
            CancellationToken cancellationToken)
        {
            CheckedFileIds.Add(fileId);
            return Task.FromResult(presence);
        }
    }
}
