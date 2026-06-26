using Nexus.SharedKernel.Domain;

namespace Nexus.Services.Identity.Domain.Onboarding;

public sealed class OnboardingSession : NexusEntity<Guid>
{
    private OnboardingSession()
    {
        Token = string.Empty;
        Email = string.Empty;
        GoogleSub = string.Empty;
        DisplayName = string.Empty;
    }

    public OnboardingSession(
        Guid id,
        string token,
        string email,
        string googleSub,
        string displayName,
        DateTimeOffset expiresAt)
    {
        Id = id;
        Token = token;
        Email = email.Trim().ToLowerInvariant();
        GoogleSub = googleSub;
        DisplayName = displayName;
        ExpiresAt = expiresAt;
    }

    public string Token { get; private set; }
    public string Email { get; private set; }
    public string GoogleSub { get; private set; }
    public string DisplayName { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }

    public bool IsExpired(DateTimeOffset now) => ExpiresAt <= now;
}
