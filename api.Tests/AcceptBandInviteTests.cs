using api.Data;
using api.Data.Entities;
using api.Handlers.Invites;
using api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests;

public class AcceptBandInviteTests
{
    [Fact]
    public async Task Matching_signed_in_email_joins_the_band()
    {
        await using var db = OpenDb();
        var member = new Member
        {
            UserIdentifier = Guid.NewGuid(),
            Username = "kit",
            Email = "kit@example.com"
        };
        var band = new Band { Name = "Kindred", CreatedDate = DateTime.UtcNow };
        db.Members.Add(member);
        db.Bands.Add(band);
        await db.SaveChangesAsync();
        db.BandInvites.Add(new BandInvite
        {
            BandId = band.Id,
            Email = "kit@example.com",
            Code = "abc12",
            RoleName = BandRoles.Uploader,
            CreatedBy = member.Id,
            CreatedDate = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        });
        await db.SaveChangesAsync();

        var result = await new AcceptBandInvite.Handler(db).Handle(
            new AcceptBandInvite.Command("kit@example.com", "abc12", member.Id),
            CancellationToken.None);

        Assert.True(result.Accepted);
        Assert.True(await db.BandMembers.AnyAsync(bandMember =>
            bandMember.BandId == band.Id &&
            bandMember.MemberId == member.Id &&
            bandMember.RoleName == BandRoles.Uploader));
    }

    [Fact]
    public async Task Guest_must_sign_in_when_email_and_code_match()
    {
        await using var db = OpenDb();
        var owner = new Member { UserIdentifier = Guid.NewGuid(), Username = "ada" };
        var band = new Band { Name = "Kindred", CreatedDate = DateTime.UtcNow };
        db.Members.Add(owner);
        db.Bands.Add(band);
        await db.SaveChangesAsync();
        db.BandInvites.Add(new BandInvite
        {
            BandId = band.Id,
            Email = "kit@example.com",
            Code = "abc12",
            RoleName = BandRoles.Member,
            CreatedBy = owner.Id,
            CreatedDate = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        });
        await db.SaveChangesAsync();

        var result = await new AcceptBandInvite.Handler(db).Handle(
            new AcceptBandInvite.Command("kit@example.com", "abc12", null),
            CancellationToken.None);

        Assert.False(result.Accepted);
        Assert.True(result.NeedsLogin);
        Assert.Contains("invite=abc12", result.LoginUrl, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Wrong_email_is_rejected()
    {
        await using var db = OpenDb();
        var owner = new Member { UserIdentifier = Guid.NewGuid(), Username = "ada" };
        var band = new Band { Name = "Kindred", CreatedDate = DateTime.UtcNow };
        db.Members.Add(owner);
        db.Bands.Add(band);
        await db.SaveChangesAsync();
        db.BandInvites.Add(new BandInvite
        {
            BandId = band.Id,
            Email = "kit@example.com",
            Code = "abc12",
            RoleName = BandRoles.Member,
            CreatedBy = owner.Id,
            CreatedDate = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        });
        await db.SaveChangesAsync();

        var result = await new AcceptBandInvite.Handler(db).Handle(
            new AcceptBandInvite.Command("other@example.com", "abc12", null),
            CancellationToken.None);

        Assert.False(result.Accepted);
        Assert.Equal("Email does not match this invite.", result.Error);
    }

    private static DatabaseContext OpenDb()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new DatabaseContext(options);
    }
}
