using Nexus.Services.Identity.Contracts.Onboarding;
using Nexus.Services.Identity.Contracts.Users;

namespace Nexus.Services.Identity.Contracts.Onboarding;

public interface IOnboardingAppService
{
    Task<GoogleAuthResultDto> GoogleAuthAsync(GoogleAuthDto input, CancellationToken cancellationToken = default);
    Task<PreviewTenantCodeResultDto> PreviewCodeAsync(PreviewTenantCodeDto input, CancellationToken cancellationToken = default);
    Task<CompleteOnboardingResultDto> CompleteAsync(CompleteOnboardingDto input, CancellationToken cancellationToken = default);
    Task<LoginResultDto> LoginEmailAsync(LoginEmailDto input, CancellationToken cancellationToken = default);
    Task<TenantByEmailResultDto?> GetTenantByEmailAsync(string email, CancellationToken cancellationToken = default);
}
