using api.Contracts;
using api.Data;
using api.Services;
using MediatR;

namespace api.Handlers.Albums;

public static class GetProposal
{
    public record Query(int AlbumId, int ProposalId, int MemberId) : IRequest<AlbumActionResult<AlbumProposalDto>>;

    public class Handler(DatabaseContext db) : IRequestHandler<Query, AlbumActionResult<AlbumProposalDto>>
    {
        public async Task<AlbumActionResult<AlbumProposalDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var album = await LoadAlbum.ById(db, request.AlbumId, cancellationToken);
            if (album is null)
            {
                return new AlbumActionResult<AlbumProposalDto>("not_found", null);
            }

            var members = await LoadAlbum.BandMembers(db, album.BandId, cancellationToken);
            if (members.All(member => member.MemberId != request.MemberId))
            {
                return new AlbumActionResult<AlbumProposalDto>("not_in_band", null);
            }

            var proposal = album.Proposals.FirstOrDefault(item => item.Id == request.ProposalId);
            if (proposal is null)
            {
                return new AlbumActionResult<AlbumProposalDto>("not_found", null);
            }

            return new AlbumActionResult<AlbumProposalDto>(
                null,
                AlbumDtoMapper.ToProposal(proposal, album.ApprovalRule, members));
        }
    }
}
