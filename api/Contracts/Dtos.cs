namespace api.Contracts;

public record MemberSummaryDto(
    int Id,
    Guid UserIdentifier,
    string Username,
    string? Fullname,
    string? Image);

public record BandSummaryDto(
    int Id,
    string Name,
    string? Genre,
    string? Image);

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
    IReadOnlyList<BandSummaryDto> Bands,
    IReadOnlyList<RoleSummaryDto> Roles);

public record SongDto(
    int Id,
    string Name,
    string? Description,
    int UploadedBy,
    int? Version,
    int PreviousVersion,
    string StorageKind);

public record GoogleStatusDto(
    bool Configured,
    bool Connected,
    string? Email);

public record DriveFileDto(
    string Id,
    string Name,
    string? MimeType);
