namespace api.Contracts;

public record MemberSummaryDto(
    int Id,
    Guid UserIdentifier,
    string Username,
    string? Fullname,
    string? Image,
    string? Email);

public record BandSummaryDto(
    int Id,
    string Name,
    string? Genre,
    string? Image,
    string? DriveFolderId,
    string? DriveFolderName,
    string? MyRole,
    bool IsOwner);

public record RoleSummaryDto(
    int Id,
    string RoleName,
    int BandId);

public record MemberDto(
    int Id,
    Guid UserIdentifier,
    string Username,
    string? Fullname,
    string? Image,
    string? Email,
    IReadOnlyList<BandSummaryDto> Bands,
    IReadOnlyList<RoleSummaryDto> Roles);

public record SongDto(
    int Id,
    string Name,
    string? Description,
    int UploadedBy,
    int? Version,
    int PreviousVersion,
    string StorageKind,
    string? ContentMd5,
    DateTime? SourceModifiedAt);

public record GoogleStatusDto(
    bool Configured,
    bool SignedIn,
    bool Connected,
    string? Email,
    MemberDto? Member);

public record DriveFileDto(
    string Id,
    string Name,
    string? MimeType,
    string? Md5 = null,
    DateTime? ModifiedAt = null);

public record AddDriveSongRequest(
    string DriveFileId,
    string? Name);

public record SetDriveFolderRequest(
    string FolderId,
    string? Name);

public record CreateInviteRequest(
    string Email,
    string? Role);

public record InviteDto(
    int Id,
    string Email,
    string RoleName,
    string Code,
    string AcceptUrl,
    bool EmailSent,
    DateTime ExpiresAt);

public record AcceptInviteRequest(
    string Email,
    string Code);

public record AcceptInviteResultDto(
    bool Accepted,
    bool NeedsLogin,
    string? LoginUrl,
    string? Error,
    int? BandId);

public record PickerTokenDto(
    string AccessToken,
    string ClientId,
    string? ApiKey);

public record CreateBandRequest(string Name);

public record AlbumActionResult<T>(string? Error, T? Value);

public record AlbumSummaryDto(
    int Id,
    int BandId,
    string Name,
    bool Archived,
    string ApprovalRule,
    int TrackCount,
    int OpenProposalCount);

public record InclusionTallyDto(
    string? MyChoice,
    int InCount,
    int OutCount,
    int AbstainCount);

public record NameCandidateDto(
    string Name,
    int VoteCount,
    bool IsMine);

public record NameContestDto(
    string? MyName,
    IReadOnlyList<NameCandidateDto> Candidates);

public record OrderTrackTallyDto(
    int? MyRank,
    double? AverageRank,
    int? ConsensusPosition);

public record OrderContestDto(
    bool Locked,
    int VoteCount,
    IReadOnlyList<int>? MySongIds);

public record ArtCandidateDto(
    string DriveFileId,
    int VoteCount,
    bool IsMine);

public record ArtContestDto(
    bool Locked,
    string? AppliedDriveFileId,
    int VoteCount,
    string? MyDriveFileId,
    IReadOnlyList<ArtCandidateDto> Candidates);

public record AlbumTrackDto(
    int Id,
    int SongId,
    string SongName,
    int? Version,
    int SortOrder,
    DateTime AddedAt,
    InclusionTallyDto Inclusion,
    NameContestDto Names,
    OrderTrackTallyDto Order);

public record OrderVoteRequest(IReadOnlyList<int> SongIds);

public record OrderLockRequest(bool Locked);

public record ArtVoteRequest(string DriveFileId);

public record ArtLockRequest(bool Locked);

public record AlbumArtUploadDto(
    string DriveFileId,
    string Name,
    string? MimeType,
    int? Width,
    int? Height,
    string? Warning);

public record NameVoteRequest(
    int? SongId,
    string Name);

public record InclusionVoteRequest(
    int SongId,
    string Choice);

public record ProposalDecisionDto(
    int MemberId,
    string MemberName,
    string Decision,
    DateTime UpdatedAt);

public record WaitingMemberDto(
    int MemberId,
    string Name);

public record ProposalReviewDto(
    int Id,
    int MemberId,
    string MemberName,
    int StartMs,
    int EndMs,
    string Body,
    DateTime CreatedAt);

public record AlbumProposalDto(
    int Id,
    int AlbumId,
    int SongId,
    string SongName,
    int? SongVersion,
    int ProposedBy,
    string ProposedByName,
    string Status,
    DateTime CreatedAt,
    DateTime? ResolvedAt,
    IReadOnlyList<ProposalDecisionDto> Decisions,
    IReadOnlyList<WaitingMemberDto> WaitingOn,
    IReadOnlyList<ProposalReviewDto> Reviews);

public record AlbumDetailDto(
    int Id,
    int BandId,
    string Name,
    bool Archived,
    string ApprovalRule,
    IReadOnlyList<AlbumTrackDto> Tracks,
    IReadOnlyList<AlbumProposalDto> Proposals,
    NameContestDto Names,
    OrderContestDto Order,
    ArtContestDto Art);

public record CreateAlbumRequest(
    string Name,
    string? ApprovalRule);

public record UpdateAlbumRequest(
    string? Name,
    bool? Archived,
    string? ApprovalRule);

public record CreateProposalRequest(int SongId);

public record ProposalDecisionRequest(string Decision);

public record CreateProposalReviewRequest(
    int StartMs,
    int? EndMs,
    string Body);
