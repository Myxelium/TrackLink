using api.Contracts;
using api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace api.Handlers.Members;

public static class ListMembers
{
    public record Query : IRequest<IReadOnlyList<MemberSummaryDto>>;

    public class Handler(DatabaseContext db) : IRequestHandler<Query, IReadOnlyList<MemberSummaryDto>>
    {
        public async Task<IReadOnlyList<MemberSummaryDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await db.Members
                .AsNoTracking()
                .OrderBy(m => m.Username)
                .Select(m => new MemberSummaryDto(m.Id, m.UserIdentifier, m.Username, m.Fullname, m.Image))
                .ToListAsync(cancellationToken);
        }
    }
}
