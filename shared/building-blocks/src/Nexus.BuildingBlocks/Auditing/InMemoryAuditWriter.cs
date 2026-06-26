using System.Collections.Concurrent;
using Nexus.SharedKernel.Auditing;

namespace Nexus.BuildingBlocks.Auditing;

public sealed class InMemoryAuditWriter : IAuditWriter
{
    private readonly ConcurrentQueue<AuditLogEntry> _entries = new();

    public IReadOnlyCollection<AuditLogEntry> Entries => _entries.ToArray();

    public Task WriteAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        _entries.Enqueue(entry);
        return Task.CompletedTask;
    }
}
