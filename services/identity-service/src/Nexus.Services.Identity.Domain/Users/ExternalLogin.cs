using Nexus.SharedKernel.Domain;

namespace Nexus.Services.Identity.Domain.Users;

public sealed class ExternalLogin : NexusEntity<Guid>
{
    private ExternalLogin()
    {
        Provider = string.Empty;
        ProviderKey = string.Empty;
    }

    public ExternalLogin(Guid id, Guid userId, string provider, string providerKey)
    {
        Id = id;
        UserId = userId;
        Provider = provider;
        ProviderKey = providerKey;
    }

    public Guid UserId { get; private set; }
    public string Provider { get; private set; }
    public string ProviderKey { get; private set; }
}
