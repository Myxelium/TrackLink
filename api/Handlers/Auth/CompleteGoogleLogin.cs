using System.Text.RegularExpressions;
using api.Contracts;
using api.Data;
using api.Data.Entities;
using api.Integrations.Google;
using api.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace api.Handlers.Auth;

public static class CompleteGoogleLogin
{
    public record Command(string Code, string? InviteCode) : IRequest<int?>;

    public class Handler(DatabaseContext db, IGoogleDriveService googleDrive) : IRequestHandler<Command, int?>
    {
        public async Task<int?> Handle(Command request, CancellationToken cancellationToken)
        {
            var profile = await googleDrive.ExchangeCodeAsync(request.Code, cancellationToken);
            if (profile is null || string.IsNullOrWhiteSpace(profile.Email))
            {
                return null;
            }

            var email = profile.Email.Trim();
            var member = await db.Members
                .Include(existing => existing.BandMembers)
                .FirstOrDefaultAsync(
                    existing => existing.Email == email ||
                                (profile.Subject != null && existing.GoogleSubject == profile.Subject),
                    cancellationToken);

            if (member is null)
            {
                member = new Member
                {
                    UserIdentifier = Guid.NewGuid(),
                    Username = await UniqueUsernameAsync(email, cancellationToken),
                    Fullname = TrimName(profile.Name),
                    Email = email,
                    GoogleSubject = profile.Subject,
                    Image = TrimImage(profile.Picture)
                };
                db.Members.Add(member);
                await db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                member.Email = email;
                member.GoogleSubject = profile.Subject ?? member.GoogleSubject;
                if (!string.IsNullOrWhiteSpace(profile.Name))
                {
                    member.Fullname = TrimName(profile.Name);
                }

                if (!string.IsNullOrWhiteSpace(profile.Picture))
                {
                    member.Image = TrimImage(profile.Picture);
                }

                await db.SaveChangesAsync(cancellationToken);
            }

            await googleDrive.SaveTokensAsync(member.Id, profile, cancellationToken);

            if (!await db.BandMembers.AnyAsync(bandMember => bandMember.MemberId == member.Id, cancellationToken))
            {
                await AttachFirstBandAsync(member.Id, cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(request.InviteCode))
            {
                await TryAcceptInviteAsync(member, request.InviteCode, cancellationToken);
            }

            return member.Id;
        }

        private async Task AttachFirstBandAsync(int memberId, CancellationToken cancellationToken)
        {
            var kindred = await db.Bands.OrderBy(band => band.Id).FirstOrDefaultAsync(cancellationToken);
            if (kindred is null)
            {
                return;
            }

            var hasGoogleOwner = kindred.OwnerMemberId is int ownerId &&
                await db.GoogleAccounts.AnyAsync(
                    account => account.MemberId == ownerId,
                    cancellationToken);

            var roleName = hasGoogleOwner ? BandRoles.Member : BandRoles.Owner;
            db.BandMembers.Add(new BandMember
            {
                BandId = kindred.Id,
                MemberId = memberId,
                RoleName = roleName
            });

            if (!hasGoogleOwner)
            {
                kindred.OwnerMemberId = memberId;
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        private async Task TryAcceptInviteAsync(Member member, string code, CancellationToken cancellationToken)
        {
            var invite = await db.BandInvites.FirstOrDefaultAsync(
                pending => pending.Code == code.Trim() && pending.AcceptedAt == null,
                cancellationToken);
            if (invite is null ||
                !string.Equals(invite.Email, member.Email, StringComparison.OrdinalIgnoreCase) ||
                invite.ExpiresAt < DateTime.UtcNow)
            {
                return;
            }

            if (!await db.BandMembers.AnyAsync(
                    bandMember => bandMember.BandId == invite.BandId && bandMember.MemberId == member.Id,
                    cancellationToken))
            {
                db.BandMembers.Add(new BandMember
                {
                    BandId = invite.BandId,
                    MemberId = member.Id,
                    RoleName = BandRoles.IsKnown(invite.RoleName) ? invite.RoleName : BandRoles.Member
                });
            }

            invite.AcceptedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }

        private async Task<string> UniqueUsernameAsync(string email, CancellationToken cancellationToken)
        {
            var local = email.Split('@')[0];
            var cleaned = Regex.Replace(local, "[^a-zA-Z0-9._-]", "");
            if (cleaned.Length == 0)
            {
                cleaned = "member";
            }

            if (cleaned.Length > 40)
            {
                cleaned = cleaned[..40];
            }

            var candidate = cleaned;
            var suffix = 2;
            while (await db.Members.AnyAsync(existing => existing.Username == candidate, cancellationToken))
            {
                candidate = $"{cleaned}{suffix}";
                suffix++;
            }

            return candidate;
        }

        private static string? TrimName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var trimmed = name.Trim();
            return trimmed.Length > 50 ? trimmed[..50] : trimmed;
        }

        private static string? TrimImage(string? picture)
        {
            if (string.IsNullOrWhiteSpace(picture))
            {
                return null;
            }

            var trimmed = picture.Trim();
            return trimmed.Length > 500 ? trimmed[..500] : trimmed;
        }
    }
}
