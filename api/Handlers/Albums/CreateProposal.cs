using api.Contracts;
using api.Data;
using api.Data.Entities;
using api.Integrations.Google;
using api.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace api.Handlers.Albums;

public static class CreateProposal
{
    public record Command(int AlbumId, int MemberId, int SongId) : IRequest<AlbumActionResult<AlbumProposalDto>>;

    public class Handler(DatabaseContext db, IGoogleDriveService googleDrive)
        : IRequestHandler<Command, AlbumActionResult<AlbumProposalDto>>
    {
        public async Task<AlbumActionResult<AlbumProposalDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var album = await LoadAlbum.ById(db, request.AlbumId, cancellationToken);
            if (album is null)
            {
                return new AlbumActionResult<AlbumProposalDto>("not_found", null);
            }

            if (album.Archived)
            {
                return new AlbumActionResult<AlbumProposalDto>("archived", null);
            }

            var members = await LoadAlbum.BandMembers(db, album.BandId, cancellationToken);
            var membership = members.FirstOrDefault(member => member.MemberId == request.MemberId);
            if (membership is null)
            {
                return new AlbumActionResult<AlbumProposalDto>("not_in_band", null);
            }

            if (!BandRoles.CanManageAlbums(membership.RoleName))
            {
                return new AlbumActionResult<AlbumProposalDto>("forbidden", null);
            }

            var onBand = await db.SongIdentifiers.AnyAsync(
                identifier => identifier.BandId == album.BandId && identifier.SongId == request.SongId,
                cancellationToken);
            if (!onBand)
            {
                return new AlbumActionResult<AlbumProposalDto>("song_missing", null);
            }

            if (album.Tracks.Any(track => track.SongId == request.SongId))
            {
                return new AlbumActionResult<AlbumProposalDto>("already_on_album", null);
            }

            if (album.Proposals.Any(proposal =>
                    proposal.SongId == request.SongId && proposal.Status == AlbumProposalStatuses.Open))
            {
                return new AlbumActionResult<AlbumProposalDto>("already_open", null);
            }

            var song = await db.Songs.FirstOrDefaultAsync(catalog => catalog.Id == request.SongId, cancellationToken);
            if (song is null)
            {
                return new AlbumActionResult<AlbumProposalDto>("song_missing", null);
            }

            if (song.Url.StartsWith(AudioPlaybackService.DrivePrefix, StringComparison.OrdinalIgnoreCase))
            {
                var folderId = album.Band.DriveFolderId;
                if (string.IsNullOrWhiteSpace(folderId))
                {
                    return new AlbumActionResult<AlbumProposalDto>("folder_missing", null);
                }

                var fileId = song.Url[AudioPlaybackService.DrivePrefix.Length..];
                if (!await googleDrive.IsFileInsideFolderAsync(
                        request.MemberId,
                        fileId,
                        folderId,
                        cancellationToken))
                {
                    return new AlbumActionResult<AlbumProposalDto>("outside_folder", null);
                }
            }

            var proposal = new AlbumProposal
            {
                AlbumId = album.Id,
                SongId = song.Id,
                ProposedBy = request.MemberId,
                Status = AlbumProposalStatuses.Open,
                CreatedAt = DateTime.UtcNow,
                Song = song,
                ProposedByNavigation = membership.Member,
                Album = album
            };
            proposal.Decisions.Add(new AlbumProposalDecision
            {
                MemberId = request.MemberId,
                Decision = AlbumProposalDecisions.Approve,
                UpdatedAt = DateTime.UtcNow,
                Member = membership.Member
            });
            db.AlbumProposals.Add(proposal);
            AlbumDtoMapper.AdmitIfApproved(
                album,
                proposal,
                AlbumDtoMapper.Evaluate(proposal, album.ApprovalRule, members));
            await db.SaveChangesAsync(cancellationToken);

            return new AlbumActionResult<AlbumProposalDto>(
                null,
                AlbumDtoMapper.ToProposal(proposal, album.ApprovalRule, members));
        }
    }
}
