using Microsoft.EntityFrameworkCore;
using Nexus.Services.Identity.Domain.Onboarding;
using Nexus.Services.Identity.Domain.Users;
using Nexus.Services.Identity.Infrastructure.Persistence;

namespace Nexus.Services.Identity.Infrastructure.Onboarding;

public sealed class EfCoreOnboardingRepository : IOnboardingRepository
{
    private readonly IdentityDbContext _db;

    public EfCoreOnboardingRepository(IdentityDbContext db)
    {
        _db = db;
    }

    public Task<TenantRegistration?> FindRegistrationByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _db.TenantRegistrations.SingleOrDefaultAsync(x => x.Email == normalized, cancellationToken);
    }

    public Task<TenantRegistration?> FindRegistrationByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return _db.TenantRegistrations.SingleOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
    }

    public Task<OnboardingSession?> FindSessionByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return _db.OnboardingSessions.SingleOrDefaultAsync(x => x.Token == token, cancellationToken);
    }

    public Task<ExternalLogin?> FindExternalLoginAsync(string provider, string providerKey, CancellationToken cancellationToken = default)
    {
        return _db.ExternalLogins.SingleOrDefaultAsync(x => x.Provider == provider && x.ProviderKey == providerKey, cancellationToken);
    }

    public async Task InsertRegistrationAsync(TenantRegistration registration, CancellationToken cancellationToken = default)
    {
        await _db.TenantRegistrations.AddAsync(registration, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task InsertSessionAsync(OnboardingSession session, CancellationToken cancellationToken = default)
    {
        await _db.OnboardingSessions.AddAsync(session, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task InsertExternalLoginAsync(ExternalLogin externalLogin, CancellationToken cancellationToken = default)
    {
        await _db.ExternalLogins.AddAsync(externalLogin, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteSessionAsync(OnboardingSession session, CancellationToken cancellationToken = default)
    {
        _db.OnboardingSessions.Remove(session);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
