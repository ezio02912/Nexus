namespace Nexus.SharedKernel.Auditing;

public sealed record AuditLogEntry(
    Guid Id,
    Guid? TenantId,
    Guid? UserId,
    string ServiceName,
    string EntityName,
    string? EntityId,
    AuditAction Action,
    string? Summary,
    string? CorrelationId,
    DateTimeOffset OccurredAt);
