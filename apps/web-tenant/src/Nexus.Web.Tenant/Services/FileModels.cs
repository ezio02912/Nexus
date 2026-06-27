namespace Nexus.Web.Tenant.Services;

public sealed record FileDto(
    Guid Id,
    Guid? TenantId,
    string FileName,
    string ContentType,
    long Size,
    string StoragePath,
    DateTimeOffset CreatedAt);

public sealed record FileLinkRecord(
    Guid Id,
    Guid FileId,
    string Module,
    string EntityType,
    string EntityId,
    DateTimeOffset CreatedAt,
    string FileName,
    string ContentType,
    long Size);

public sealed record CreateFileLinkRequest(Guid FileId, string Module, string EntityType, string EntityId);
