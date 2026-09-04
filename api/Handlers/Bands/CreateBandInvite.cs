using System.Security.Cryptography;
using api.Contracts;
using api.Data;
using api.Data.Entities;
using api.Integrations.Google;
using api.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace api.Handlers.Bands;

public static class CreateBandInvite
{
    public record Command(int BandId, int MemberId, string Email, string? Role) : IRequest<InviteDto?>;

    public class Handler(
        DatabaseContext db,
        IInviteMailer mailer,
        IOptions<GoogleOptions> googleOptions) : IRequestHandler<Command, InviteDto?>
    {
        public async Task<InviteDto?> Handle(Command request, CancellationToken cancellationToken)
        {
            var membership = await db.BandMembers
                .Include(bandMember => bandMember.Band)
                .FirstOrDefaultAsync(
                    bandMember => bandMember.BandId == request.BandId && bandMember.MemberId == request.MemberId,
                    cancellationToken);
            if (membership is null || !BandRoles.CanInvite(membership.RoleName))
            {
                return null;
            }

            var email = request.Email.Trim();
            if (email.Length is 0 or > 320 || !email.Contains('@', StringComparison.Ordinal))
            {
                return null;
            }

            var roleName = BandRoles.IsKnown(request.Role) && request.Role != BandRoles.Owner
                ? request.Role!
                : BandRoles.Member;

            var invite = new BandInvite
            {
                BandId = request.BandId,
                Email = email.ToLowerInvariant(),
                Code = NewCode(),
                RoleName = roleName,
                CreatedBy = request.MemberId,
                CreatedDate = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(14)
            };
            db.BandInvites.Add(invite);
            await db.SaveChangesAsync(cancellationToken);

            var acceptUrl = BuildAcceptUrl(googleOptions.Value.AppReturnUrl, invite.Code, invite.Email);
            var emailed = await mailer.SendInviteAsync(
                invite.Email,
                membership.Band.Name,
                acceptUrl,
                invite.Code,
                cancellationToken);

            return new InviteDto(
                invite.Id,
                invite.Email,
                invite.RoleName,
                invite.Code,
                acceptUrl,
                emailed,
                invite.ExpiresAt);
        }

        private static string NewCode()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(5)).ToLowerInvariant();
        }

        private static string BuildAcceptUrl(string appReturnUrl, string code, string email)
        {
            var root = appReturnUrl.TrimEnd('/');
            return $"{root}/join?code={Uri.EscapeDataString(code)}&email={Uri.EscapeDataString(email)}";
        }
    }
}
