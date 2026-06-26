using Nexus.SharedKernel.Domain;

namespace Nexus.Services.Identity.Domain.Users;

public sealed class RefreshToken : NexusEntity<Guid>
{
    private RefreshToken()
    {
        TokenHash = string.Empty;
    }

    public RefreshToken(Guid id, Guid tenantId, Guid userId, string tokenHash, DateTimeOffset expiresAt, DateTimeOffset createdAt)
    {
        Id = id;
        TenantId = tenantId;
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
    }

    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;

    public void Revoke(DateTimeOffset now) => RevokedAt = now;
}
