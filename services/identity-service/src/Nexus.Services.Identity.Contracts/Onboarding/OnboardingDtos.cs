namespace Nexus.Services.Identity.Contracts.Onboarding;

using Nexus.Services.Identity.Contracts.Users;

public sealed class GoogleAuthDto
{
    public string? IdToken { get; init; }
    public string? AccessToken { get; init; }
}

public sealed class GoogleAuthResultDto
{
    public required string Status { get; init; }
    public string? OnboardingToken { get; init; }
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
    public LoginResultDto? Login { get; init; }
}

public sealed class PreviewTenantCodeDto
{
    public required string CompanyName { get; init; }
}

public sealed class PreviewTenantCodeResultDto
{
    public required string SuggestedCode { get; init; }
    public required bool Available { get; init; }
}

public sealed class CompleteOnboardingDto
{
    public required string OnboardingToken { get; init; }
    public required string CompanyName { get; init; }
    public required string Code { get; init; }
    public required string RepresentativeName { get; init; }
    public string? Address { get; init; }
    public string? Phone { get; init; }
    public string? UserName { get; init; }
    public string? Password { get; init; }
}

public sealed class CompleteOnboardingResultDto
{
    public required Guid TenantId { get; init; }
    public required string TenantCode { get; init; }
    public required string TenantName { get; init; }
    public required LoginResultDto Login { get; init; }
}

public sealed class LoginEmailDto
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}

public sealed class TenantByEmailResultDto
{
    public required Guid TenantId { get; init; }
    public required string TenantCode { get; init; }
    public required string TenantName { get; init; }
}
