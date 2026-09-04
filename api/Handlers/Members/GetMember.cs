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
                .Where(m => m.Id == request.Id)
                .Select(m => new MemberDto(
                    m.Id,
                    m.UserIdentifier,
                    m.Username,
                    m.Fullname,
                    m.Image,
                    m.BandMembers.Select(bm => new BandSummaryDto(
                        bm.Band.Id,
                        bm.Band.Name,
                        bm.Band.Genre,
                        bm.Band.Image)).ToList(),
                    m.MemberRoles.Select(mr => new RoleSummaryDto(
                        mr.Role.Id,
                        mr.Role.RoleName,
                        mr.Role.CreatedFor)).ToList()))
                .FirstOrDefaultAsync(cancellationToken);

            return member;
        }
    }
}
