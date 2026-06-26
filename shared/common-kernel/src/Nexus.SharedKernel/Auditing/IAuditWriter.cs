namespace Nexus.SharedKernel.Auditing;

public interface IAuditWriter
{
    Task WriteAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
}
