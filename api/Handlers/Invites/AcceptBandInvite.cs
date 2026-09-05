using api.Contracts;
using api.Data;
using api.Data.Entities;
using api.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace api.Handlers.Invites;

public static class AcceptBandInvite
{
    public record Command(string Email, string Code, int? MemberId) : IRequest<AcceptInviteResultDto>;

    public class Handler(DatabaseContext db) : IRequestHandler<Command, AcceptInviteResultDto>
    {
        public async Task<AcceptInviteResultDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var email = request.Email.Trim();
            var code = request.Code.Trim();
            var invite = await db.BandInvites.FirstOrDefaultAsync(
                pending => pending.Code == code,
                cancellationToken);

            if (invite is null || (invite.AcceptedAt is null && invite.ExpiresAt < DateTime.UtcNow))
            {
                return new AcceptInviteResultDto(false, false, null, "Invite is missing or expired.", null);
            }

            if (!string.Equals(invite.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                return new AcceptInviteResultDto(false, false, null, "Email does not match this invite.", null);
            }

            if (invite.AcceptedAt is not null)
            {
                return new AcceptInviteResultDto(true, false, null, null, invite.BandId);
            }

            if (request.MemberId is null)
            {
                var loginUrl = "/api/auth/google/login?invite=" + Uri.EscapeDataString(invite.Code);
                return new AcceptInviteResultDto(false, true, loginUrl, null, invite.BandId);
            }

            var member = await db.Members.FirstOrDefaultAsync(
                existing => existing.Id == request.MemberId,
                cancellationToken);
            if (member?.Email is null ||
                !string.Equals(member.Email, invite.Email, StringComparison.OrdinalIgnoreCase))
            {
                return new AcceptInviteResultDto(
                    false,
                    false,
                    null,
                    "Sign in with the Google account that matches this invite.",
                    invite.BandId);
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
            return new AcceptInviteResultDto(true, false, null, null, invite.BandId);
        }
    }
}
