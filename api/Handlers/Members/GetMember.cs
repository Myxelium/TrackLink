using api.Contracts;
using api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace api.Handlers.Members;

public static class GetMember
{
    public record Query(int Id) : IRequest<MemberDto?>;

    public class Handler(DatabaseContext db) : IRequestHandler<Query, MemberDto?>
    {
        public async Task<MemberDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            var member = await db.Members
                .AsNoTracking()
                .Where(existing => existing.Id == request.Id)
                .Select(existing => new MemberDto(
                    existing.Id,
                    existing.UserIdentifier,
                    existing.Username,
                    existing.Fullname,
                    existing.Image,
                    existing.Email,
                    existing.BandMembers.Select(bandMember => new BandSummaryDto(
                        bandMember.Band.Id,
                        bandMember.Band.Name,
                        bandMember.Band.Genre,
                        bandMember.Band.Image,
                        bandMember.Band.DriveFolderId,
                        bandMember.Band.DriveFolderName,
                        bandMember.RoleName,
                        bandMember.Band.OwnerMemberId == existing.Id)).ToList(),
                    existing.MemberRoles.Select(memberRole => new RoleSummaryDto(
                        memberRole.Role.Id,
                        memberRole.Role.RoleName,
                        memberRole.Role.CreatedFor)).ToList()))
                .FirstOrDefaultAsync(cancellationToken);

            return member;
        }
    }
}
