using Nexus.Services.Identity.Domain.Onboarding;
using Nexus.Services.Identity.Domain.Users;

namespace Nexus.Services.Identity.Domain.Onboarding;

public interface IOnboardingRepository
{
    Task<TenantRegistration?> FindRegistrationByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<TenantRegistration?> FindRegistrationByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<OnboardingSession?> FindSessionByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<ExternalLogin?> FindExternalLoginAsync(string provider, string providerKey, CancellationToken cancellationToken = default);
    Task InsertRegistrationAsync(TenantRegistration registration, CancellationToken cancellationToken = default);
    Task InsertSessionAsync(OnboardingSession session, CancellationToken cancellationToken = default);
    Task InsertExternalLoginAsync(ExternalLogin externalLogin, CancellationToken cancellationToken = default);
    Task DeleteSessionAsync(OnboardingSession session, CancellationToken cancellationToken = default);
}
