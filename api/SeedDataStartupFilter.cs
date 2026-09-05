using api.Data;
using api.Data.Entities;
using api.Services;
using Microsoft.EntityFrameworkCore;

namespace api;

public class SeedDataStartupFilter(IConfiguration configuration) : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            var logger = app.ApplicationServices.GetRequiredService<ILogger<SeedDataStartupFilter>>();
            try
            {
                if (configuration.GetValue<bool>("Seed"))
                {
                    using var scope = app.ApplicationServices.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                    Seed(db);
                }

                next(app);
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "Database seed failed");
                throw;
            }
        };
    }

    private static void Seed(DatabaseContext db)
    {
        if (db.Members.Any())
        {
            return;
        }

        var ada = new Member
        {
            UserIdentifier = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeee1"),
            Username = "ada",
            Fullname = "Ada Vale"
        };
        var kit = new Member
        {
            UserIdentifier = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeee2"),
            Username = "kit",
            Fullname = "Kit Morrow"
        };
        db.Members.AddRange(ada, kit);
        db.SaveChanges();

        var band = new Band
        {
            Name = "Kindred",
            Genre = "indie",
            CreatedDate = DateTime.UtcNow
        };
        db.Bands.Add(band);
        db.SaveChanges();

        band.OwnerMemberId = ada.Id;
        db.BandMembers.AddRange(
            new BandMember { BandId = band.Id, MemberId = ada.Id, RoleName = BandRoles.Owner },
            new BandMember { BandId = band.Id, MemberId = kit.Id, RoleName = BandRoles.Member });

        db.Roles.Add(new Role
        {
            CreatedBy = ada.Id,
            CreatedFor = band.Id,
            RoleName = "Producer",
            RoleDescription = "Session producer",
            CreatedDate = DateTime.UtcNow
        });
        db.SaveChanges();

        var producer = db.Roles.AsNoTracking().OrderBy(r => r.Id).First();
        db.MemberRoles.Add(new MemberRole { MemberId = ada.Id, RoleId = producer.Id });

        var takeOne = new Song
        {
            Name = "Night Shift",
            Description = "Demo take streamed by id",
            UploadedBy = ada.Id,
            Version = 1,
            PreviousVersion = 0,
            Url = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3"
        };
        var takeTwo = new Song
        {
            Name = "Wireframe",
            Description = "Second demo take",
            UploadedBy = kit.Id,
            Version = 1,
            PreviousVersion = 0,
            Url = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-2.mp3"
        };
        db.Songs.AddRange(takeOne, takeTwo);
        db.SaveChanges();

        db.SongIdentifiers.AddRange(
            new SongIdentifier { BandId = band.Id, SongId = takeOne.Id },
            new SongIdentifier { BandId = band.Id, SongId = takeTwo.Id });
        db.SaveChanges();
    }
}
