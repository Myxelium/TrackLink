using api.Contracts;
using api.Data;
using api.Data.Entities;
using api.Handlers.Albums;
using api.Integrations.Google;
using api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests;

public class GetAlbumArtTests
{
    [Fact]
    public async Task Streams_the_applied_cover_when_it_is_an_image_inside_the_band_folder()
    {
        var fx = await OpenBand("cover-dawn");
        var drive = new PreviewDrive(["cover-dawn"], "cover-dawn", "image/jpeg", [1, 2, 3]);

        var opened = await new GetAlbumArt.Handler(fx.Db, drive).Handle(
            new GetAlbumArt.Query(fx.Album.Id, fx.Owner.Id),
            CancellationToken.None);

        Assert.Null(opened.Error);
        Assert.Equal("image/jpeg", opened.ContentType);
        Assert.Equal([1, 2, 3], ReadBytes(opened.Stream!));
        Assert.Equal(1, drive.DownloadCount);
        Assert.Equal(["cover-dawn"], drive.DownloadedFileIds);
    }

    [Fact]
    public async Task Rejects_a_cover_outside_the_band_folder_without_downloading()
    {
        var fx = await OpenBand("cover-outside");
        var drive = new PreviewDrive(["cover-dawn"], "cover-outside", "image/jpeg", [9]);

        var opened = await new GetAlbumArt.Handler(fx.Db, drive).Handle(
            new GetAlbumArt.Query(fx.Album.Id, fx.Owner.Id),
            CancellationToken.None);

        Assert.Equal("outside_folder", opened.Error);
        Assert.Null(opened.Stream);
        Assert.Equal(0, drive.DownloadCount);
    }

    [Fact]
    public async Task Rejects_when_no_cover_is_applied()
    {
        var fx = await OpenBand(null);
        var drive = new PreviewDrive(["cover-dawn"], "cover-dawn", "image/jpeg", [1]);

        var opened = await new GetAlbumArt.Handler(fx.Db, drive).Handle(
            new GetAlbumArt.Query(fx.Album.Id, fx.Owner.Id),
            CancellationToken.None);

        Assert.Equal("art_missing", opened.Error);
        Assert.Equal(0, drive.DownloadCount);
    }

    [Fact]
    public async Task Rejects_a_non_image_cover_without_downloading()
    {
        var fx = await OpenBand("take-night");
        var drive = new PreviewDrive(["take-night"], "take-night", "audio/mpeg", [4, 5]);

        var opened = await new GetAlbumArt.Handler(fx.Db, drive).Handle(
            new GetAlbumArt.Query(fx.Album.Id, fx.Owner.Id),
            CancellationToken.None);

        Assert.Equal("not_image", opened.Error);
        Assert.Equal(0, drive.DownloadCount);
    }

    [Fact]
    public async Task Streams_a_candidate_image_by_file_id_without_an_applied_cover()
    {
        var fx = await OpenBand(null);
        var drive = new PreviewDrive(["cover-dawn"], "cover-dawn", "image/png", [8, 8, 8]);

        var opened = await new GetAlbumArt.Handler(fx.Db, drive).Handle(
            new GetAlbumArt.Query(fx.Album.Id, fx.Owner.Id, "cover-dawn"),
            CancellationToken.None);

        Assert.Null(opened.Error);
        Assert.Equal("image/png", opened.ContentType);
        Assert.Equal([8, 8, 8], ReadBytes(opened.Stream!));
        Assert.Equal(["cover-dawn"], drive.DownloadedFileIds);
    }

    [Fact]
    public async Task Candidate_preview_rejects_a_file_outside_the_band_folder_without_downloading()
    {
        var fx = await OpenBand("cover-dawn");
        var drive = new PreviewDrive(["cover-dawn"], "cover-outside", "image/jpeg", [9]);

        var opened = await new GetAlbumArt.Handler(fx.Db, drive).Handle(
            new GetAlbumArt.Query(fx.Album.Id, fx.Owner.Id, "cover-outside"),
            CancellationToken.None);

        Assert.Equal("outside_folder", opened.Error);
        Assert.Null(opened.Stream);
        Assert.Equal(0, drive.DownloadCount);
    }

    [Fact]
    public async Task Candidate_preview_rejects_a_non_image_without_downloading()
    {
        var fx = await OpenBand(null);
        var drive = new PreviewDrive(["take-night"], "take-night", "audio/mpeg", [4, 5]);

        var opened = await new GetAlbumArt.Handler(fx.Db, drive).Handle(
            new GetAlbumArt.Query(fx.Album.Id, fx.Owner.Id, "take-night"),
            CancellationToken.None);

        Assert.Equal("not_image", opened.Error);
        Assert.Equal(0, drive.DownloadCount);
    }

    [Fact]
    public async Task Candidate_preview_rejects_an_invalid_file_id()
    {
        var fx = await OpenBand("cover-dawn");
        var drive = new PreviewDrive(["cover-dawn"], "cover-dawn", "image/jpeg", [1]);

        var opened = await new GetAlbumArt.Handler(fx.Db, drive).Handle(
            new GetAlbumArt.Query(fx.Album.Id, fx.Owner.Id, "../outside"),
            CancellationToken.None);

        Assert.Equal("invalid_art", opened.Error);
        Assert.Equal(0, drive.DownloadCount);
    }

    [Fact]
    public async Task Rejects_a_member_outside_the_band()
    {
        var fx = await OpenBand("cover-dawn");
        var stranger = new Member { UserIdentifier = Guid.NewGuid(), Username = "out" };
        fx.Db.Members.Add(stranger);
        await fx.Db.SaveChangesAsync();
        var drive = new PreviewDrive(["cover-dawn"], "cover-dawn", "image/jpeg", [1]);

        var opened = await new GetAlbumArt.Handler(fx.Db, drive).Handle(
            new GetAlbumArt.Query(fx.Album.Id, stranger.Id),
            CancellationToken.None);

        Assert.Equal("not_in_band", opened.Error);
        Assert.Equal(0, drive.DownloadCount);
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
        return new BandFixture(db, owner, band, album);
    }

    private static byte[] ReadBytes(Stream stream)
    {
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    private sealed record BandFixture(DatabaseContext Db, Member Owner, Band Band, Album Album);

    private sealed class PreviewDrive(
        IReadOnlyCollection<string> insideFileIds,
        string listedFileId,
        string mimeType,
        byte[] bytes) : IGoogleDriveService
    {
        public int DownloadCount { get; private set; }

        public List<string> DownloadedFileIds { get; } = [];

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
            Task.FromResult(insideFileIds.Contains(fileId));

        public Task<DriveFileDto?> GetAudioFileAsync(
            int memberId,
            string fileId,
            CancellationToken cancellationToken) =>
            Task.FromResult<DriveFileDto?>(
                fileId == listedFileId
                    ? new DriveFileDto(fileId, fileId, mimeType)
                    : null);

        public Task<DriveDownload?> DownloadAsync(
            int memberId,
            string fileId,
            CancellationToken cancellationToken)
        {
            DownloadCount++;
            DownloadedFileIds.Add(fileId);
            if (!insideFileIds.Contains(fileId) || fileId != listedFileId)
            {
                return Task.FromResult<DriveDownload?>(null);
            }

            return Task.FromResult<DriveDownload?>(new DriveDownload
            {
                Stream = new MemoryStream(bytes),
                ContentType = mimeType
            });
        }
    }
}
