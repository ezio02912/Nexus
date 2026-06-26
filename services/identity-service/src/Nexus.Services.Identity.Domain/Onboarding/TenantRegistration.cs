using Nexus.SharedKernel.Domain;

namespace Nexus.Services.Identity.Domain.Onboarding;

public sealed class TenantRegistration : NexusEntity<Guid>
{
    private TenantRegistration()
    {
        Email = string.Empty;
    }

    public TenantRegistration(Guid id, string email, Guid tenantId, Guid userId, DateTimeOffset createdAt)
    {
        Id = id;
        Email = email.Trim().ToLowerInvariant();
        TenantId = tenantId;
        UserId = userId;
        CreatedAt = createdAt;
    }

    public string Email { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
