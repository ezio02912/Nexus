using Nexus.SharedKernel.Context;
using Nexus.SharedKernel.Exceptions;

namespace Nexus.Services.Tenant.Domain.Tenants;

public sealed class TenantManager
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ICurrentUser _currentUser;

    public TenantManager(ITenantRepository tenantRepository, ICurrentUser currentUser)
    {
        _tenantRepository = tenantRepository;
        _currentUser = currentUser;
    }

    public async Task<Tenant> CreateAsync(string code, string name, CancellationToken cancellationToken = default)
    {
        return await CreateAsync(code, name, null, null, string.Empty, string.Empty, cancellationToken);
    }

    public async Task<Tenant> CreateAsync(
        string code,
        string name,
        string? address,
        string? phone,
        string representativeName,
        string contactEmail,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = Tenant.NormalizeCode(code);
        var existing = await _tenantRepository.FindByCodeAsync(normalizedCode, cancellationToken);
        if (existing is not null)
        {
            throw new NexusBusinessException(TenantErrorCodes.AlreadyExists, $"Tenant code '{normalizedCode}' already exists.");
        }

        var tenant = new Tenant(
            Guid.NewGuid(),
            normalizedCode,
            name,
            address,
            phone,
            representativeName,
            contactEmail,
            _currentUser.Id,
            DateTimeOffset.UtcNow);
        return await _tenantRepository.InsertAsync(tenant, cancellationToken);
    }
}
