using api.Contracts;
using api.Data;
using api.Data.Entities;
using api.Handlers.Albums;
using api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests;

public class AlbumDeskTests
{
    [Fact]
    public async Task Owner_creates_an_album_with_all_member_rule()
    {
        var fx = await OpenBand();

        var album = await new CreateAlbum.Handler(fx.Db).Handle(
            new CreateAlbum.Command(fx.Band.Id, fx.Owner.Id, "First Light", null),
            CancellationToken.None);

        Assert.NotNull(album);
        Assert.Null(album.Error);
        Assert.Equal("First Light", album.Value!.Name);
        Assert.Equal(AlbumApproval.All, album.Value.ApprovalRule);
        Assert.False(album.Value.Archived);
    }

    [Fact]
    public async Task Member_cannot_create_an_album()
    {
        var fx = await OpenBand();

        var album = await new CreateAlbum.Handler(fx.Db).Handle(
            new CreateAlbum.Command(fx.Band.Id, fx.Listener.Id, "Secret", null),
            CancellationToken.None);

        Assert.Equal("forbidden", album.Error);
        Assert.False(await fx.Db.Albums.AnyAsync());
    }

    [Fact]
    public async Task All_members_must_approve_before_the_take_joins_the_album()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.All);
        var proposal = await Propose(fx, album.Id, fx.Uploader.Id);

        Assert.Equal(AlbumProposalStatuses.Open, proposal.Value!.Status);
        Assert.Empty(await fx.Db.AlbumTracks.ToListAsync());

        var first = await Decide(fx, proposal.Value.Id, fx.Owner.Id, AlbumProposalDecisions.Approve);
        Assert.Equal(AlbumProposalStatuses.Open, first.Value!.Status);

        var second = await Decide(fx, proposal.Value.Id, fx.Uploader.Id, AlbumProposalDecisions.Approve);
        Assert.Equal(AlbumProposalStatuses.Open, second.Value!.Status);

        var last = await Decide(fx, proposal.Value.Id, fx.Listener.Id, AlbumProposalDecisions.Approve);
        Assert.Equal(AlbumProposalStatuses.Approved, last.Value!.Status);
        Assert.True(await fx.Db.AlbumTracks.AnyAsync(track =>
            track.AlbumId == album.Id && track.SongId == fx.Song.Id));
    }

    [Fact]
    public async Task Required_reject_keeps_the_take_off_the_album()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.All);
        var proposal = await Propose(fx, album.Id, fx.Owner.Id);

        var rejected = await Decide(fx, proposal.Value!.Id, fx.Listener.Id, AlbumProposalDecisions.Reject);

        Assert.Equal(AlbumProposalStatuses.Rejected, rejected.Value!.Status);
        Assert.False(await fx.Db.AlbumTracks.AnyAsync());
    }

    [Fact]
    public async Task Owner_uploaders_rule_merges_without_a_listener_vote()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.OwnerUploaders);
        var proposal = await Propose(fx, album.Id, fx.Owner.Id);

        await Decide(fx, proposal.Value!.Id, fx.Owner.Id, AlbumProposalDecisions.Approve);
        var merged = await Decide(fx, proposal.Value.Id, fx.Uploader.Id, AlbumProposalDecisions.Approve);

        Assert.Equal(AlbumProposalStatuses.Approved, merged.Value!.Status);
        Assert.True(await fx.Db.AlbumTracks.AnyAsync(track => track.SongId == fx.Song.Id));
    }

    [Fact]
    public async Task Changing_a_reject_to_approve_can_still_merge()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.OwnerUploaders);
        var proposal = await Propose(fx, album.Id, fx.Owner.Id);

        await Decide(fx, proposal.Value!.Id, fx.Owner.Id, AlbumProposalDecisions.Approve);
        await Decide(fx, proposal.Value.Id, fx.Uploader.Id, AlbumProposalDecisions.Reject);
        var merged = await Decide(fx, proposal.Value.Id, fx.Uploader.Id, AlbumProposalDecisions.Approve);

        Assert.Equal(AlbumProposalStatuses.Approved, merged.Value!.Status);
        Assert.True(await fx.Db.AlbumTracks.AnyAsync());
    }

    [Fact]
    public async Task Withdraw_leaves_the_song_in_the_band_catalog_only()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.All);
        var proposal = await Propose(fx, album.Id, fx.Uploader.Id);

        var withdrawn = await new WithdrawProposal.Handler(fx.Db).Handle(
            new WithdrawProposal.Command(album.Id, proposal.Value!.Id, fx.Uploader.Id),
            CancellationToken.None);

        Assert.Null(withdrawn.Error);
        Assert.Equal(AlbumProposalStatuses.Withdrawn, withdrawn.Value!.Status);
        Assert.False(await fx.Db.AlbumTracks.AnyAsync());
        Assert.True(await fx.Db.SongIdentifiers.AnyAsync(identifier => identifier.SongId == fx.Song.Id));
    }

    [Fact]
    public async Task Solo_voter_propose_lands_the_take_with_the_new_proposal_id()
    {
        var fx = await OpenBand();
        fx.Owner.Email = "ada@example.com";
        var album = await CreateOpenAlbum(fx, AlbumApproval.All);
        var proposal = await Propose(fx, album.Id, fx.Owner.Id);

        Assert.Null(proposal.Error);
        Assert.Equal(AlbumProposalStatuses.Approved, proposal.Value!.Status);
        var track = Assert.Single(await fx.Db.AlbumTracks.ToListAsync());
        Assert.True(proposal.Value.Id > 0);
        Assert.Equal(proposal.Value.Id, track.ProposalId);
    }

    [Fact]
    public async Task Band_member_can_pin_a_timestamped_review_on_an_open_proposal()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.All);
        var proposal = await Propose(fx, album.Id, fx.Owner.Id);

        var reviewed = await new AddProposalReview.Handler(fx.Db).Handle(
            new AddProposalReview.Command(album.Id, proposal.Value!.Id, fx.Listener.Id, 12500, 18300, "Snare is late"),
            CancellationToken.None);

        Assert.Null(reviewed.Error);
        var note = Assert.Single(reviewed.Value!.Reviews);
        Assert.Equal(fx.Listener.Id, note.MemberId);
        Assert.Equal(12500, note.StartMs);
        Assert.Equal(18300, note.EndMs);
        Assert.Equal("Snare is late", note.Body);
        Assert.Equal(AlbumProposalStatuses.Open, reviewed.Value.Status);
    }

    [Fact]
    public async Task Approved_proposal_still_accepts_a_timestamped_review()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.OwnerUploaders);
        var proposal = await Propose(fx, album.Id, fx.Owner.Id);
        await Decide(fx, proposal.Value!.Id, fx.Uploader.Id, AlbumProposalDecisions.Approve);

        var reviewed = await new AddProposalReview.Handler(fx.Db).Handle(
            new AddProposalReview.Command(album.Id, proposal.Value.Id, fx.Owner.Id, 4000, 9000, "Chorus vocal is thin"),
            CancellationToken.None);

        Assert.Null(reviewed.Error);
        Assert.Equal(AlbumProposalStatuses.Approved, reviewed.Value!.Status);
        Assert.Equal("Chorus vocal is thin", Assert.Single(reviewed.Value.Reviews).Body);
    }

    [Fact]
    public async Task Review_without_a_comment_is_rejected()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.All);
        var proposal = await Propose(fx, album.Id, fx.Owner.Id);

        var reviewed = await new AddProposalReview.Handler(fx.Db).Handle(
            new AddProposalReview.Command(album.Id, proposal.Value!.Id, fx.Owner.Id, 0, 1000, "   "),
            CancellationToken.None);

        Assert.Equal("invalid_review", reviewed.Error);
        Assert.False(await fx.Db.AlbumProposalReviews.AnyAsync());
    }

    [Fact]
    public async Task Author_can_remove_their_review()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.All);
        var proposal = await Propose(fx, album.Id, fx.Owner.Id);
        var reviewed = await new AddProposalReview.Handler(fx.Db).Handle(
            new AddProposalReview.Command(album.Id, proposal.Value!.Id, fx.Uploader.Id, 0, 500, "Too dry"),
            CancellationToken.None);

        var removed = await new RemoveProposalReview.Handler(fx.Db).Handle(
            new RemoveProposalReview.Command(album.Id, proposal.Value.Id, reviewed.Value!.Reviews[0].Id, fx.Uploader.Id),
            CancellationToken.None);

        Assert.Null(removed.Error);
        Assert.Empty(removed.Value!.Reviews);
    }

    private static async Task<AlbumSummaryDto> CreateOpenAlbum(BandFixture fx, string rule)
    {
        var created = await new CreateAlbum.Handler(fx.Db).Handle(
            new CreateAlbum.Command(fx.Band.Id, fx.Owner.Id, "First Light", rule),
            CancellationToken.None);
        return created.Value!;
    }

    private static Task<AlbumActionResult<AlbumProposalDto>> Propose(BandFixture fx, int albumId, int memberId)
    {
        return new CreateProposal.Handler(fx.Db, new AllowAllDrive()).Handle(
            new CreateProposal.Command(albumId, memberId, fx.Song.Id),
            CancellationToken.None);
    }

    private static Task<AlbumActionResult<AlbumProposalDto>> Decide(
        BandFixture fx,
        int proposalId,
        int memberId,
        string decision)
    {
        return new DecideProposal.Handler(fx.Db).Handle(
            new DecideProposal.Command(fx.Db.AlbumProposals.Single(proposal => proposal.Id == proposalId).AlbumId, proposalId, memberId, decision),
            CancellationToken.None);
    }

    private static async Task<BandFixture> OpenBand()
    {
        var db = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        var owner = new Member { UserIdentifier = Guid.NewGuid(), Username = "ada", Fullname = "Ada Vale" };
        var uploader = new Member { UserIdentifier = Guid.NewGuid(), Username = "kit", Fullname = "Kit Reed" };
        var listener = new Member { UserIdentifier = Guid.NewGuid(), Username = "moe", Fullname = "Moe Vale" };
        var band = new Band { Name = "Kindred", CreatedDate = DateTime.UtcNow };
        var song = new Song
        {
            Name = "Night Shift",
            Description = "Demo",
            UploadedByNavigation = owner,
            Version = 1,
            PreviousVersion = 0,
            Url = "https://example.com/night-shift.mp3"
        };
        db.Members.AddRange(owner, uploader, listener);
        db.Bands.Add(band);
        db.Songs.Add(song);
        await db.SaveChangesAsync();
        band.OwnerMemberId = owner.Id;
        db.BandMembers.AddRange(
            new BandMember { BandId = band.Id, MemberId = owner.Id, RoleName = BandRoles.Owner },
            new BandMember { BandId = band.Id, MemberId = uploader.Id, RoleName = BandRoles.Uploader },
            new BandMember { BandId = band.Id, MemberId = listener.Id, RoleName = BandRoles.Member });
        db.SongIdentifiers.Add(new SongIdentifier { BandId = band.Id, SongId = song.Id });
        await db.SaveChangesAsync();
        return new BandFixture(db, owner, uploader, listener, band, song);
    }

    private sealed record BandFixture(
        DatabaseContext Db,
        Member Owner,
        Member Uploader,
        Member Listener,
        Band Band,
        Song Song);

    private sealed class AllowAllDrive : api.Integrations.Google.IGoogleDriveService
    {
        public bool IsConfigured => true;

        public Task<string?> CreateAuthorizationUrlAsync(string? state, CancellationToken cancellationToken) =>
            Task.FromResult<string?>(null);

        public Task<api.Integrations.Google.GoogleLoginProfile?> ExchangeCodeAsync(
            string code,
            CancellationToken cancellationToken) =>
            Task.FromResult<api.Integrations.Google.GoogleLoginProfile?>(null);

        public Task SaveTokensAsync(
            int memberId,
            api.Integrations.Google.GoogleLoginProfile profile,
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

        public Task<bool> CanReadFolderAsync(int memberId, string folderId, CancellationToken cancellationToken) =>
            Task.FromResult(true);

        public Task<bool> IsFileInsideFolderAsync(
            int memberId,
            string fileId,
            string folderId,
            CancellationToken cancellationToken) =>
            Task.FromResult(true);

        public Task<api.Contracts.DriveFileDto?> GetAudioFileAsync(
            int memberId,
            string fileId,
            CancellationToken cancellationToken) =>
            Task.FromResult<api.Contracts.DriveFileDto?>(null);

        public Task<api.Integrations.Google.DriveDownload?> DownloadAsync(
            int memberId,
            string fileId,
            CancellationToken cancellationToken) =>
            Task.FromResult<api.Integrations.Google.DriveDownload?>(null);
    }
}
