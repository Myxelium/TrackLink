using api.Contracts;
using api.Data;
using api.Integrations.Google;
using api.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace api.Handlers.Auth;

public static class GetSession
{
    public record Query(int? MemberId) : IRequest<GoogleStatusDto>;

    public class Handler(DatabaseContext db, IGoogleDriveService googleDrive) : IRequestHandler<Query, GoogleStatusDto>
    {
        public async Task<GoogleStatusDto> Handle(Query request, CancellationToken cancellationToken)
        {
            if (!googleDrive.IsConfigured)
            {
                return new GoogleStatusDto(false, false, false, null, null);
            }

            if (request.MemberId is null)
            {
                return new GoogleStatusDto(true, false, false, null, null);
            }

            var member = await db.Members
                .AsNoTracking()
                .Include(existing => existing.BandMembers)
                    .ThenInclude(bandMember => bandMember.Band)
                .Include(existing => existing.MemberRoles)
                    .ThenInclude(memberRole => memberRole.Role)
                .FirstOrDefaultAsync(existing => existing.Id == request.MemberId, cancellationToken);

            if (member is null)
            {
                return new GoogleStatusDto(true, false, false, null, null);
            }

            var connected = await googleDrive.HasTokensAsync(member.Id, cancellationToken);
            var email = member.Email ?? await googleDrive.GetEmailAsync(member.Id, cancellationToken);
            return new GoogleStatusDto(true, true, connected, email, MemberDtoMapper.ToDto(member));
        }
    }
}
