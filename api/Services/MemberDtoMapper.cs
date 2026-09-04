using api.Contracts;
using api.Data.Entities;

namespace api.Services;

public static class MemberDtoMapper
{
    public static MemberDto ToDto(Member member)
    {
        return new MemberDto(
            member.Id,
            member.UserIdentifier,
            member.Username,
            member.Fullname,
            member.Image,
            member.Email,
            member.BandMembers.Select(bandMember => new BandSummaryDto(
                bandMember.Band.Id,
                bandMember.Band.Name,
                bandMember.Band.Genre,
                bandMember.Band.Image,
                bandMember.Band.DriveFolderId,
                bandMember.Band.DriveFolderName,
                bandMember.RoleName,
                bandMember.Band.OwnerMemberId == member.Id)).ToList(),
            member.MemberRoles.Select(memberRole => new RoleSummaryDto(
                memberRole.Role.Id,
                memberRole.Role.RoleName,
                memberRole.Role.CreatedFor)).ToList());
    }
}
