using Nexus.Services.Identity.Domain.Onboarding;

namespace Nexus.Services.Identity.Domain.Tests.Onboarding;

public sealed class OnboardingSessionTests
{
    [Fact]
    public void Constructor_normalizes_email()
    {
        var session = new OnboardingSession(
            Guid.NewGuid(),
            "token",
            "  Admin@Example.COM  ",
            "google-sub",
            "Admin",
            DateTimeOffset.UtcNow.AddMinutes(15));

        Assert.Equal("admin@example.com", session.Email);
    }

    [Fact]
    public void IsExpired_returns_true_at_or_after_expiry_time()
    {
        var expiresAt = new DateTimeOffset(2026, 6, 27, 8, 0, 0, TimeSpan.Zero);
        var session = new OnboardingSession(Guid.NewGuid(), "token", "admin@example.com", "google-sub", "Admin", expiresAt);

        Assert.False(session.IsExpired(expiresAt.AddTicks(-1)));
        Assert.True(session.IsExpired(expiresAt));
        Assert.True(session.IsExpired(expiresAt.AddSeconds(1)));
    }
}
