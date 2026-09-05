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

    [Fact]
    public async Task Inclusion_vote_does_not_admit_or_remove_a_take()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.OwnerUploaders);
        var proposal = await Propose(fx, album.Id, fx.Owner.Id);
        await Decide(fx, proposal.Value!.Id, fx.Uploader.Id, AlbumProposalDecisions.Approve);

        var votedIn = await new CastInclusionVote.Handler(fx.Db).Handle(
            new CastInclusionVote.Command(album.Id, fx.Listener.Id, fx.Song.Id, "in"),
            CancellationToken.None);

        Assert.Null(votedIn.Error);
        var inTrack = Assert.Single(votedIn.Value!.Tracks);
        Assert.Equal("in", inTrack.Inclusion.MyChoice);
        Assert.Equal(1, inTrack.Inclusion.InCount);
        Assert.Equal(1, await fx.Db.AlbumTracks.CountAsync());

        var votedOut = await new CastInclusionVote.Handler(fx.Db).Handle(
            new CastInclusionVote.Command(album.Id, fx.Listener.Id, fx.Song.Id, "OUT"),
            CancellationToken.None);

        var outTrack = Assert.Single(votedOut.Value!.Tracks);
        Assert.Equal("out", outTrack.Inclusion.MyChoice);
        Assert.Equal(1, outTrack.Inclusion.OutCount);
        Assert.Equal(0, outTrack.Inclusion.InCount);
        Assert.True(await fx.Db.AlbumTracks.AnyAsync(track => track.SongId == fx.Song.Id));
        Assert.Equal(1, await fx.Db.Votes.CountAsync());
        Assert.Equal(
            AlbumProposalStatuses.Approved,
            await fx.Db.AlbumProposals.Where(item => item.Id == proposal.Value.Id).Select(item => item.Status).SingleAsync());
    }

    [Fact]
    public async Task Inclusion_vote_cannot_land_a_song_that_is_only_proposed()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.All);
        await Propose(fx, album.Id, fx.Owner.Id);

        var voted = await new CastInclusionVote.Handler(fx.Db).Handle(
            new CastInclusionVote.Command(album.Id, fx.Owner.Id, fx.Song.Id, "in"),
            CancellationToken.None);

        Assert.Equal("not_on_album", voted.Error);
        Assert.False(await fx.Db.Votes.AnyAsync());
        Assert.False(await fx.Db.AlbumTracks.AnyAsync());
    }

    [Fact]
    public async Task Album_name_vote_tallies_without_renaming()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.OwnerUploaders);

        var voted = await new CastNameVote.Handler(fx.Db).Handle(
            new CastNameVote.Command(album.Id, fx.Owner.Id, null, "  Midnight Sun  "),
            CancellationToken.None);

        Assert.Null(voted.Error);
        Assert.Equal("First Light", voted.Value!.Name);
        Assert.Equal("Midnight Sun", voted.Value.Names.MyName);
        var suggested = Assert.Single(voted.Value.Names.Candidates, candidate => candidate.IsMine);
        Assert.Equal("Midnight Sun", suggested.Name);
        Assert.Equal(1, suggested.VoteCount);
        Assert.Contains(voted.Value.Names.Candidates, candidate => candidate.Name == "First Light");
        Assert.Equal("First Light", await fx.Db.Albums.Where(item => item.Id == album.Id).Select(item => item.Name).SingleAsync());
        Assert.Equal(1, await fx.Db.Votes.CountAsync());
        Assert.Equal(AlbumVoteKinds.AlbumName, await fx.Db.Votes.Select(vote => vote.Kind).SingleAsync());
    }

    [Fact]
    public async Task Song_name_vote_tallies_without_renaming_or_touching_inclusion()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.OwnerUploaders);
        var proposal = await Propose(fx, album.Id, fx.Owner.Id);
        await Decide(fx, proposal.Value!.Id, fx.Uploader.Id, AlbumProposalDecisions.Approve);

        await new CastInclusionVote.Handler(fx.Db).Handle(
            new CastInclusionVote.Command(album.Id, fx.Listener.Id, fx.Song.Id, "in"),
            CancellationToken.None);

        var voted = await new CastNameVote.Handler(fx.Db).Handle(
            new CastNameVote.Command(album.Id, fx.Listener.Id, fx.Song.Id, "Night Shift (radio)"),
            CancellationToken.None);

        Assert.Null(voted.Error);
        var track = Assert.Single(voted.Value!.Tracks);
        Assert.Equal("Night Shift", track.SongName);
        Assert.Equal("in", track.Inclusion.MyChoice);
        Assert.Equal("Night Shift (radio)", track.Names.MyName);
        Assert.Equal(1, track.Names.Candidates.Single(candidate => candidate.IsMine).VoteCount);
        Assert.Equal("Night Shift", await fx.Db.Songs.Where(song => song.Id == fx.Song.Id).Select(song => song.Name).SingleAsync());
        Assert.Equal(2, await fx.Db.Votes.CountAsync());
        Assert.Equal(1, await fx.Db.AlbumTracks.CountAsync());
    }

    [Fact]
    public async Task Song_name_vote_cannot_land_on_a_proposed_only_take()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.All);
        await Propose(fx, album.Id, fx.Owner.Id);

        var voted = await new CastNameVote.Handler(fx.Db).Handle(
            new CastNameVote.Command(album.Id, fx.Owner.Id, fx.Song.Id, "Other Title"),
            CancellationToken.None);

        Assert.Equal("not_on_album", voted.Error);
        Assert.False(await fx.Db.Votes.AnyAsync());
    }

    [Fact]
    public async Task Name_vote_rejects_a_blank_title()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.All);

        var voted = await new CastNameVote.Handler(fx.Db).Handle(
            new CastNameVote.Command(album.Id, fx.Owner.Id, null, "   "),
            CancellationToken.None);

        Assert.Equal("invalid_title", voted.Error);
        Assert.False(await fx.Db.Votes.AnyAsync());
    }

    [Fact]
    public async Task Changing_a_name_vote_moves_the_ballot()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.All);

        await new CastNameVote.Handler(fx.Db).Handle(
            new CastNameVote.Command(album.Id, fx.Owner.Id, null, "Dawn Chorus"),
            CancellationToken.None);
        var moved = await new CastNameVote.Handler(fx.Db).Handle(
            new CastNameVote.Command(album.Id, fx.Owner.Id, null, "first light"),
            CancellationToken.None);

        Assert.Null(moved.Error);
        Assert.Equal("First Light", moved.Value!.Names.MyName);
        Assert.Equal(1, await fx.Db.Votes.CountAsync());
        Assert.Equal(1, moved.Value.Names.Candidates.Single(candidate => candidate.Name == "First Light").VoteCount);
        Assert.DoesNotContain(moved.Value.Names.Candidates, candidate => candidate.Name == "Dawn Chorus");
    }

    [Fact]
    public async Task Member_can_rank_admitted_takes_and_lock_applies_that_order()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.OwnerUploaders);
        var second = await AddBandSong(fx, "Dawn Chorus");
        await Admit(fx, album.Id, fx.Song.Id);
        await Admit(fx, album.Id, second.Id);

        var ranked = await new CastOrderVote.Handler(fx.Db).Handle(
            new CastOrderVote.Command(album.Id, fx.Listener.Id, [second.Id, fx.Song.Id]),
            CancellationToken.None);

        Assert.Null(ranked.Error);
        Assert.False(ranked.Value!.Order.Locked);
        Assert.Equal(1, ranked.Value.Order.VoteCount);
        Assert.Equal([second.Id, fx.Song.Id], ranked.Value.Order.MySongIds);
        Assert.Equal([fx.Song.Id, second.Id], ranked.Value.Tracks.Select(track => track.SongId).ToArray());
        Assert.Equal(1, ranked.Value.Tracks.Single(track => track.SongId == second.Id).Order.ConsensusPosition);
        Assert.Equal(1, ranked.Value.Tracks.Single(track => track.SongId == second.Id).Order.MyRank);
        Assert.Equal(2, ranked.Value.Tracks.Single(track => track.SongId == fx.Song.Id).Order.ConsensusPosition);
        Assert.Equal(1, await fx.Db.AlbumTracks.Where(track => track.SongId == fx.Song.Id).Select(track => track.SortOrder).SingleAsync());

        var locked = await new LockAlbumOrder.Handler(fx.Db).Handle(
            new LockAlbumOrder.Command(album.Id, fx.Owner.Id, true),
            CancellationToken.None);

        Assert.Null(locked.Error);
        Assert.True(locked.Value!.Order.Locked);
        Assert.Equal([second.Id, fx.Song.Id], locked.Value.Tracks.Select(track => track.SongId).ToArray());
        Assert.Equal(1, await fx.Db.AlbumTracks.Where(track => track.SongId == second.Id).Select(track => track.SortOrder).SingleAsync());
        Assert.Equal(2, await fx.Db.AlbumTracks.Where(track => track.SongId == fx.Song.Id).Select(track => track.SortOrder).SingleAsync());

        var fetched = await new GetAlbum.Handler(fx.Db, new AllowAllDrive()).Handle(
            new GetAlbum.Query(album.Id, fx.Listener.Id),
            CancellationToken.None);

        Assert.Equal([second.Id, fx.Song.Id], fetched.Value!.Tracks.Select(track => track.SongId).ToArray());
        Assert.True(fetched.Value.Order.Locked);
    }

    [Fact]
    public async Task Locked_order_rejects_further_rank_changes()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.OwnerUploaders);
        var second = await AddBandSong(fx, "Dawn Chorus");
        await Admit(fx, album.Id, fx.Song.Id);
        await Admit(fx, album.Id, second.Id);
        await new CastOrderVote.Handler(fx.Db).Handle(
            new CastOrderVote.Command(album.Id, fx.Owner.Id, [second.Id, fx.Song.Id]),
            CancellationToken.None);
        await new LockAlbumOrder.Handler(fx.Db).Handle(
            new LockAlbumOrder.Command(album.Id, fx.Owner.Id, true),
            CancellationToken.None);

        var rejected = await new CastOrderVote.Handler(fx.Db).Handle(
            new CastOrderVote.Command(album.Id, fx.Listener.Id, [fx.Song.Id, second.Id]),
            CancellationToken.None);

        Assert.Equal("order_locked", rejected.Error);
        Assert.Equal(2, await fx.Db.Votes.CountAsync(vote => vote.Kind == AlbumVoteKinds.Order));
    }

    [Fact]
    public async Task Listener_cannot_lock_the_track_order()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.OwnerUploaders);
        await Admit(fx, album.Id, fx.Song.Id);

        var locked = await new LockAlbumOrder.Handler(fx.Db).Handle(
            new LockAlbumOrder.Command(album.Id, fx.Listener.Id, true),
            CancellationToken.None);

        Assert.Equal("forbidden", locked.Error);
        Assert.False(await fx.Db.Albums.Where(item => item.Id == album.Id).Select(item => item.OrderLocked).SingleAsync());
    }

    [Fact]
    public async Task Order_vote_must_rank_every_admitted_take_once()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.OwnerUploaders);
        var second = await AddBandSong(fx, "Dawn Chorus");
        await Admit(fx, album.Id, fx.Song.Id);
        await Admit(fx, album.Id, second.Id);

        var missing = await new CastOrderVote.Handler(fx.Db).Handle(
            new CastOrderVote.Command(album.Id, fx.Owner.Id, [fx.Song.Id]),
            CancellationToken.None);
        var proposedOnly = await new CastOrderVote.Handler(fx.Db).Handle(
            new CastOrderVote.Command(album.Id, fx.Owner.Id, [fx.Song.Id, 999]),
            CancellationToken.None);

        Assert.Equal("invalid_order", missing.Error);
        Assert.Equal("invalid_order", proposedOnly.Error);
        Assert.False(await fx.Db.Votes.AnyAsync());
    }

    [Fact]
    public async Task Order_vote_cannot_rank_a_proposed_only_take()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.All);
        await Propose(fx, album.Id, fx.Owner.Id);

        var voted = await new CastOrderVote.Handler(fx.Db).Handle(
            new CastOrderVote.Command(album.Id, fx.Owner.Id, [fx.Song.Id]),
            CancellationToken.None);

        Assert.Equal("invalid_order", voted.Error);
        Assert.False(await fx.Db.Votes.AnyAsync());
    }

    [Fact]
    public async Task Changing_an_order_vote_replaces_the_ballot()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.OwnerUploaders);
        var second = await AddBandSong(fx, "Dawn Chorus");
        await Admit(fx, album.Id, fx.Song.Id);
        await Admit(fx, album.Id, second.Id);

        await new CastOrderVote.Handler(fx.Db).Handle(
            new CastOrderVote.Command(album.Id, fx.Owner.Id, [second.Id, fx.Song.Id]),
            CancellationToken.None);
        var moved = await new CastOrderVote.Handler(fx.Db).Handle(
            new CastOrderVote.Command(album.Id, fx.Owner.Id, [fx.Song.Id, second.Id]),
            CancellationToken.None);

        Assert.Null(moved.Error);
        Assert.Equal([fx.Song.Id, second.Id], moved.Value!.Order.MySongIds);
        Assert.Equal(2, await fx.Db.Votes.CountAsync(vote => vote.Kind == AlbumVoteKinds.Order));
        Assert.Equal(1, await fx.Db.Votes.CountAsync(vote =>
            vote.Kind == AlbumVoteKinds.Order && vote.MemberId == fx.Owner.Id && vote.SongId == fx.Song.Id));
    }

    [Fact]
    public async Task Order_vote_does_not_touch_inclusion_or_name_ballots()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.OwnerUploaders);
        var second = await AddBandSong(fx, "Dawn Chorus");
        await Admit(fx, album.Id, fx.Song.Id);
        await Admit(fx, album.Id, second.Id);
        await new CastInclusionVote.Handler(fx.Db).Handle(
            new CastInclusionVote.Command(album.Id, fx.Listener.Id, fx.Song.Id, "in"),
            CancellationToken.None);
        await new CastNameVote.Handler(fx.Db).Handle(
            new CastNameVote.Command(album.Id, fx.Listener.Id, null, "Midnight Sun"),
            CancellationToken.None);

        var ranked = await new CastOrderVote.Handler(fx.Db).Handle(
            new CastOrderVote.Command(album.Id, fx.Listener.Id, [second.Id, fx.Song.Id]),
            CancellationToken.None);

        Assert.Null(ranked.Error);
        Assert.Equal("in", ranked.Value!.Tracks.Single(track => track.SongId == fx.Song.Id).Inclusion.MyChoice);
        Assert.Equal("Midnight Sun", ranked.Value.Names.MyName);
        Assert.Equal("First Light", ranked.Value.Name);
        Assert.Equal(1, await fx.Db.Votes.CountAsync(vote => vote.Kind == AlbumVoteKinds.Inclusion));
        Assert.Equal(1, await fx.Db.Votes.CountAsync(vote => vote.Kind == AlbumVoteKinds.AlbumName));
        Assert.Equal(2, await fx.Db.Votes.CountAsync(vote => vote.Kind == AlbumVoteKinds.Order));
    }

    [Fact]
    public async Task Member_can_vote_on_in_folder_art_and_lock_applies_the_cover()
    {
        var fx = await OpenBand();
        fx.Band.DriveFolderId = "folder-root";
        await fx.Db.SaveChangesAsync();
        var album = await CreateOpenAlbum(fx, AlbumApproval.All);
        var drive = new FolderScopedDrive(["cover-dawn"]);

        var voted = await new CastArtVote.Handler(fx.Db, drive).Handle(
            new CastArtVote.Command(album.Id, fx.Listener.Id, "cover-dawn"),
            CancellationToken.None);

        Assert.Null(voted.Error);
        Assert.False(voted.Value!.Art.Locked);
        Assert.Null(voted.Value.Art.AppliedDriveFileId);
        Assert.Equal(1, voted.Value.Art.VoteCount);
        Assert.Equal("cover-dawn", voted.Value.Art.MyDriveFileId);
        Assert.Equal("cover-dawn", Assert.Single(voted.Value.Art.Candidates).DriveFileId);
        Assert.Null(await fx.Db.Albums.Where(item => item.Id == album.Id).Select(item => item.ArtDriveFileId).SingleAsync());

        var locked = await new LockAlbumArt.Handler(fx.Db).Handle(
            new LockAlbumArt.Command(album.Id, fx.Owner.Id, true),
            CancellationToken.None);

        Assert.Null(locked.Error);
        Assert.True(locked.Value!.Art.Locked);
        Assert.Equal("cover-dawn", locked.Value.Art.AppliedDriveFileId);
        Assert.Equal("cover-dawn", await fx.Db.Albums.Where(item => item.Id == album.Id).Select(item => item.ArtDriveFileId).SingleAsync());

        var fetched = await new GetAlbum.Handler(fx.Db, new AllowAllDrive()).Handle(
            new GetAlbum.Query(album.Id, fx.Owner.Id),
            CancellationToken.None);

        Assert.True(fetched.Value!.Art.Locked);
        Assert.Equal("cover-dawn", fetched.Value.Art.AppliedDriveFileId);
    }

    [Fact]
    public async Task Art_vote_rejects_a_file_outside_the_band_folder()
    {
        var fx = await OpenBand();
        fx.Band.DriveFolderId = "folder-root";
        await fx.Db.SaveChangesAsync();
        var album = await CreateOpenAlbum(fx, AlbumApproval.All);

        var voted = await new CastArtVote.Handler(fx.Db, new FolderScopedDrive(["cover-dawn"])).Handle(
            new CastArtVote.Command(album.Id, fx.Owner.Id, "cover-outside"),
            CancellationToken.None);

        Assert.Equal("outside_folder", voted.Error);
        Assert.False(await fx.Db.Votes.AnyAsync());
    }

    [Fact]
    public async Task Art_vote_rejects_when_the_band_folder_is_missing()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.All);

        var voted = await new CastArtVote.Handler(fx.Db, new FolderScopedDrive(["cover-dawn"])).Handle(
            new CastArtVote.Command(album.Id, fx.Owner.Id, "cover-dawn"),
            CancellationToken.None);

        Assert.Equal("folder_missing", voted.Error);
        Assert.False(await fx.Db.Votes.AnyAsync());
    }

    [Fact]
    public async Task Locked_art_rejects_further_votes()
    {
        var fx = await OpenBand();
        fx.Band.DriveFolderId = "folder-root";
        await fx.Db.SaveChangesAsync();
        var album = await CreateOpenAlbum(fx, AlbumApproval.All);
        var drive = new FolderScopedDrive(["cover-dawn", "cover-dusk"]);

        await new CastArtVote.Handler(fx.Db, drive).Handle(
            new CastArtVote.Command(album.Id, fx.Owner.Id, "cover-dawn"),
            CancellationToken.None);
        await new LockAlbumArt.Handler(fx.Db).Handle(
            new LockAlbumArt.Command(album.Id, fx.Owner.Id, true),
            CancellationToken.None);

        var rejected = await new CastArtVote.Handler(fx.Db, drive).Handle(
            new CastArtVote.Command(album.Id, fx.Listener.Id, "cover-dusk"),
            CancellationToken.None);

        Assert.Equal("art_locked", rejected.Error);
        Assert.Equal(1, await fx.Db.Votes.CountAsync(vote => vote.Kind == AlbumVoteKinds.Art));
        Assert.Equal("cover-dawn", await fx.Db.Albums.Where(item => item.Id == album.Id).Select(item => item.ArtDriveFileId).SingleAsync());
    }

    [Fact]
    public async Task Listener_cannot_lock_album_art()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.All);

        var locked = await new LockAlbumArt.Handler(fx.Db).Handle(
            new LockAlbumArt.Command(album.Id, fx.Listener.Id, true),
            CancellationToken.None);

        Assert.Equal("forbidden", locked.Error);
        Assert.False(await fx.Db.Albums.Where(item => item.Id == album.Id).Select(item => item.ArtLocked).SingleAsync());
    }

    [Fact]
    public async Task Unlocking_art_reopens_voting_without_changing_the_cover()
    {
        var fx = await OpenBand();
        fx.Band.DriveFolderId = "folder-root";
        await fx.Db.SaveChangesAsync();
        var album = await CreateOpenAlbum(fx, AlbumApproval.All);
        var drive = new FolderScopedDrive(["cover-dawn", "cover-dusk"]);

        await new CastArtVote.Handler(fx.Db, drive).Handle(
            new CastArtVote.Command(album.Id, fx.Owner.Id, "cover-dawn"),
            CancellationToken.None);
        await new LockAlbumArt.Handler(fx.Db).Handle(
            new LockAlbumArt.Command(album.Id, fx.Owner.Id, true),
            CancellationToken.None);
        await new LockAlbumArt.Handler(fx.Db).Handle(
            new LockAlbumArt.Command(album.Id, fx.Owner.Id, false),
            CancellationToken.None);

        var moved = await new CastArtVote.Handler(fx.Db, drive).Handle(
            new CastArtVote.Command(album.Id, fx.Listener.Id, "cover-dusk"),
            CancellationToken.None);

        Assert.Null(moved.Error);
        Assert.False(moved.Value!.Art.Locked);
        Assert.Equal("cover-dawn", moved.Value.Art.AppliedDriveFileId);
        Assert.Equal("cover-dusk", moved.Value.Art.MyDriveFileId);
        Assert.Equal("cover-dawn", await fx.Db.Albums.Where(item => item.Id == album.Id).Select(item => item.ArtDriveFileId).SingleAsync());
    }

    [Fact]
    public async Task Art_vote_does_not_touch_inclusion_name_or_order()
    {
        var fx = await OpenBand();
        fx.Band.DriveFolderId = "folder-root";
        await fx.Db.SaveChangesAsync();
        var album = await CreateOpenAlbum(fx, AlbumApproval.OwnerUploaders);
        await Admit(fx, album.Id, fx.Song.Id);
        var second = await AddBandSong(fx, "Dawn Chorus");
        await Admit(fx, album.Id, second.Id);

        await new CastInclusionVote.Handler(fx.Db).Handle(
            new CastInclusionVote.Command(album.Id, fx.Listener.Id, fx.Song.Id, "in"),
            CancellationToken.None);
        await new CastNameVote.Handler(fx.Db).Handle(
            new CastNameVote.Command(album.Id, fx.Listener.Id, null, "Midnight Sun"),
            CancellationToken.None);
        await new CastOrderVote.Handler(fx.Db).Handle(
            new CastOrderVote.Command(album.Id, fx.Listener.Id, [second.Id, fx.Song.Id]),
            CancellationToken.None);

        var voted = await new CastArtVote.Handler(fx.Db, new FolderScopedDrive(["cover-dawn"])).Handle(
            new CastArtVote.Command(album.Id, fx.Listener.Id, "cover-dawn"),
            CancellationToken.None);

        Assert.Null(voted.Error);
        Assert.Equal("in", voted.Value!.Tracks.Single(track => track.SongId == fx.Song.Id).Inclusion.MyChoice);
        Assert.Equal("Midnight Sun", voted.Value.Names.MyName);
        Assert.Equal([second.Id, fx.Song.Id], voted.Value.Order.MySongIds);
        Assert.Equal(1, await fx.Db.Votes.CountAsync(vote => vote.Kind == AlbumVoteKinds.Inclusion));
        Assert.Equal(1, await fx.Db.Votes.CountAsync(vote => vote.Kind == AlbumVoteKinds.AlbumName));
        Assert.Equal(2, await fx.Db.Votes.CountAsync(vote => vote.Kind == AlbumVoteKinds.Order));
        Assert.Equal(1, await fx.Db.Votes.CountAsync(vote => vote.Kind == AlbumVoteKinds.Art));
    }

    [Fact]
    public async Task Invalid_art_file_id_is_rejected()
    {
        var fx = await OpenBand();
        fx.Band.DriveFolderId = "folder-root";
        await fx.Db.SaveChangesAsync();
        var album = await CreateOpenAlbum(fx, AlbumApproval.All);

        var voted = await new CastArtVote.Handler(fx.Db, new FolderScopedDrive(["cover-dawn"])).Handle(
            new CastArtVote.Command(album.Id, fx.Owner.Id, "../outside"),
            CancellationToken.None);

        Assert.Equal("invalid_art", voted.Error);
        Assert.False(await fx.Db.Votes.AnyAsync());
    }

    [Fact]
    public async Task Art_vote_rejects_a_non_image_file_in_the_band_folder()
    {
        var fx = await OpenBand();
        fx.Band.DriveFolderId = "folder-root";
        await fx.Db.SaveChangesAsync();
        var album = await CreateOpenAlbum(fx, AlbumApproval.All);

        var voted = await new CastArtVote.Handler(fx.Db, new FolderScopedDrive(["take-night"], "audio/mpeg")).Handle(
            new CastArtVote.Command(album.Id, fx.Owner.Id, "take-night"),
            CancellationToken.None);

        Assert.Equal("not_image", voted.Error);
        Assert.False(await fx.Db.Votes.AnyAsync());
    }

    [Fact]
    public async Task Unknown_inclusion_choice_is_rejected()
    {
        var fx = await OpenBand();
        var album = await CreateOpenAlbum(fx, AlbumApproval.OwnerUploaders);
        var proposal = await Propose(fx, album.Id, fx.Owner.Id);
        await Decide(fx, proposal.Value!.Id, fx.Uploader.Id, AlbumProposalDecisions.Approve);

        var voted = await new CastInclusionVote.Handler(fx.Db).Handle(
            new CastInclusionVote.Command(album.Id, fx.Owner.Id, fx.Song.Id, "approve"),
            CancellationToken.None);

        Assert.Equal("invalid_inclusion", voted.Error);
        Assert.False(await fx.Db.Votes.AnyAsync());
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
        return ProposeSong(fx, albumId, memberId, fx.Song.Id);
    }

    private static Task<AlbumActionResult<AlbumProposalDto>> ProposeSong(
        BandFixture fx,
        int albumId,
        int memberId,
        int songId)
    {
        return new CreateProposal.Handler(fx.Db, new AllowAllDrive()).Handle(
            new CreateProposal.Command(albumId, memberId, songId),
            CancellationToken.None);
    }

    private static async Task Admit(BandFixture fx, int albumId, int songId)
    {
        var proposal = await ProposeSong(fx, albumId, fx.Owner.Id, songId);
        if (proposal.Value!.Status == AlbumProposalStatuses.Approved)
        {
            return;
        }

        await Decide(fx, proposal.Value.Id, fx.Uploader.Id, AlbumProposalDecisions.Approve);
    }

    private static async Task<Song> AddBandSong(BandFixture fx, string name)
    {
        var song = new Song
        {
            Name = name,
            Description = "Demo",
            UploadedByNavigation = fx.Owner,
            Version = 1,
            PreviousVersion = 0,
            Url = $"https://example.com/{name.ToLowerInvariant().Replace(' ', '-')}.mp3"
        };
        fx.Db.Songs.Add(song);
        await fx.Db.SaveChangesAsync();
        fx.Db.SongIdentifiers.Add(new SongIdentifier { BandId = fx.Band.Id, SongId = song.Id });
        await fx.Db.SaveChangesAsync();
        return song;
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

    private sealed class FolderScopedDrive(
        IReadOnlyCollection<string> insideFileIds,
        string mimeType = "image/jpeg")
        : api.Integrations.Google.IGoogleDriveService
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
            Task.FromResult(insideFileIds.Contains(fileId));

        public Task<api.Contracts.DriveFileDto?> GetAudioFileAsync(
            int memberId,
            string fileId,
            CancellationToken cancellationToken) =>
            Task.FromResult<api.Contracts.DriveFileDto?>(
                insideFileIds.Contains(fileId)
                    ? new api.Contracts.DriveFileDto(fileId, fileId, mimeType)
                    : null);

        public Task<api.Integrations.Google.DriveDownload?> DownloadAsync(
            int memberId,
            string fileId,
            CancellationToken cancellationToken) =>
            Task.FromResult<api.Integrations.Google.DriveDownload?>(null);
    }
}
