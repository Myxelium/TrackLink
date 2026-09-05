using api.Contracts;
using api.Data;
using api.Data.Entities;
using api.Handlers.Bands;
using api.Integrations.Google;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests;

public class ListBandSongsTests
{
    [Fact]
    public async Task Hides_drive_takes_that_sit_outside_the_band_folder()
    {
        await using var db = OpenDb();
        var owner = new Member { UserIdentifier = Guid.NewGuid(), Username = "ada" };
        var band = new Band
        {
            Name = "Kindred",
            CreatedDate = DateTime.UtcNow,
            DriveFolderId = "folder-root"
        };
        var urlTake = new Song
        {
            Name = "Night Shift",
            Description = "Demo",
            UploadedByNavigation = owner,
            Version = 1,
            PreviousVersion = 0,
            Url = "https://example.com/night-shift.mp3"
        };
        var insideTake = new Song
        {
            Name = "in-folder.mp3",
            Description = "Linked from Google Drive",
            UploadedByNavigation = owner,
            Version = 1,
            PreviousVersion = 0,
            Url = "gdrive:file-inside"
        };
        var leftoverTake = new Song
        {
            Name = "bottleneck.mp3",
            Description = "Linked from Google Drive",
            UploadedByNavigation = owner,
            Version = 1,
            PreviousVersion = 0,
            Url = "gdrive:file-old-scan"
        };
        db.Members.Add(owner);
        db.Bands.Add(band);
        db.Songs.AddRange(urlTake, insideTake, leftoverTake);
        await db.SaveChangesAsync();
        db.SongIdentifiers.AddRange(
            new SongIdentifier { BandId = band.Id, SongId = urlTake.Id },
            new SongIdentifier { BandId = band.Id, SongId = insideTake.Id },
            new SongIdentifier { BandId = band.Id, SongId = leftoverTake.Id });
        await db.SaveChangesAsync();

        var listed = await new ListBandSongs.Handler(db, new FolderDrive(["file-inside"])).Handle(
            new ListBandSongs.Query(band.Id, owner.Id),
            CancellationToken.None);

        Assert.NotNull(listed);
        Assert.Equal(["in-folder.mp3", "Night Shift"], listed.Select(song => song.Name).ToArray());
    }

    [Fact]
    public async Task Unsigned_list_keeps_url_takes_and_hides_drive_takes()
    {
        await using var db = OpenDb();
        var owner = new Member { UserIdentifier = Guid.NewGuid(), Username = "ada" };
        var band = new Band
        {
            Name = "Kindred",
            CreatedDate = DateTime.UtcNow,
            DriveFolderId = "folder-root"
        };
        var urlTake = new Song
        {
            Name = "Night Shift",
            Description = "Demo",
            UploadedByNavigation = owner,
            Version = 1,
            PreviousVersion = 0,
            Url = "https://example.com/night-shift.mp3"
        };
        var leftoverTake = new Song
        {
            Name = "bottleneck.mp3",
            Description = "Linked from Google Drive",
            UploadedByNavigation = owner,
            Version = 1,
            PreviousVersion = 0,
            Url = "gdrive:file-old-scan"
        };
        db.Members.Add(owner);
        db.Bands.Add(band);
        db.Songs.AddRange(urlTake, leftoverTake);
        await db.SaveChangesAsync();
        db.SongIdentifiers.AddRange(
            new SongIdentifier { BandId = band.Id, SongId = urlTake.Id },
            new SongIdentifier { BandId = band.Id, SongId = leftoverTake.Id });
        await db.SaveChangesAsync();

        var listed = await new ListBandSongs.Handler(db, new FolderDrive(["file-old-scan"])).Handle(
            new ListBandSongs.Query(band.Id, null),
            CancellationToken.None);

        Assert.Equal("Night Shift", Assert.Single(listed!).Name);
    }

    [Fact]
    public async Task Search_matches_name_or_description_inside_the_band()
    {
        await using var db = OpenDb();
        var owner = new Member { UserIdentifier = Guid.NewGuid(), Username = "ada" };
        var kindred = new Band { Name = "Kindred", CreatedDate = DateTime.UtcNow, DriveFolderId = "folder-root" };
        var otherBand = new Band { Name = "Other", CreatedDate = DateTime.UtcNow };
        var nightShift = new Song
        {
            Name = "Night Shift",
            Description = "Demo take",
            UploadedByNavigation = owner,
            Version = 1,
            PreviousVersion = 0,
            Url = "https://example.com/night-shift.mp3"
        };
        var insideTake = new Song
        {
            Name = "in-folder.mp3",
            Description = "Linked from Google Drive",
            UploadedByNavigation = owner,
            Version = 1,
            PreviousVersion = 0,
            Url = "gdrive:file-inside"
        };
        var leftoverTake = new Song
        {
            Name = "bottleneck.mp3",
            Description = "Linked from Google Drive",
            UploadedByNavigation = owner,
            Version = 1,
            PreviousVersion = 0,
            Url = "gdrive:file-old-scan"
        };
        var otherTake = new Song
        {
            Name = "Night Walk",
            Description = "Solo",
            UploadedByNavigation = owner,
            Version = 1,
            PreviousVersion = 0,
            Url = "https://example.com/night-walk.mp3"
        };
        db.Members.Add(owner);
        db.Bands.AddRange(kindred, otherBand);
        db.Songs.AddRange(nightShift, insideTake, leftoverTake, otherTake);
        await db.SaveChangesAsync();
        db.SongIdentifiers.AddRange(
            new SongIdentifier { BandId = kindred.Id, SongId = nightShift.Id },
            new SongIdentifier { BandId = kindred.Id, SongId = insideTake.Id },
            new SongIdentifier { BandId = kindred.Id, SongId = leftoverTake.Id },
            new SongIdentifier { BandId = otherBand.Id, SongId = otherTake.Id });
        await db.SaveChangesAsync();

        var handler = new ListBandSongs.Handler(db, new FolderDrive(["file-inside"]));

        var byName = await handler.Handle(new ListBandSongs.Query(kindred.Id, owner.Id, "NIGHT"), CancellationToken.None);
        Assert.Equal("Night Shift", Assert.Single(byName!).Name);

        var byNotes = await handler.Handle(new ListBandSongs.Query(kindred.Id, owner.Id, "demo"), CancellationToken.None);
        Assert.Equal("Night Shift", Assert.Single(byNotes!).Name);

        var leftoverName = await handler.Handle(
            new ListBandSongs.Query(kindred.Id, owner.Id, "bottleneck"),
            CancellationToken.None);
        Assert.Empty(leftoverName!);

        var otherBandName = await handler.Handle(
            new ListBandSongs.Query(kindred.Id, owner.Id, "Walk"),
            CancellationToken.None);
        Assert.Empty(otherBandName!);

        var blankQuery = await handler.Handle(
            new ListBandSongs.Query(kindred.Id, owner.Id, "  "),
            CancellationToken.None);
        Assert.Equal(["in-folder.mp3", "Night Shift"], blankQuery!.Select(song => song.Name).ToArray());
    }

    private static DatabaseContext OpenDb()
    {
        return new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
    }

    private sealed class FolderDrive(IReadOnlyCollection<string> insideFileIds) : IGoogleDriveService
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
                insideFileIds.Contains(fileId)
                    ? new DriveFileDto(fileId, fileId, "audio/mpeg")
                    : null);

        public Task<DriveDownload?> DownloadAsync(
            int memberId,
            string fileId,
            CancellationToken cancellationToken) =>
            Task.FromResult<DriveDownload?>(null);
    }
}
