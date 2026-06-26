namespace Nexus.Services.Identity.Application.Onboarding;

public sealed record GoogleTokenPayload(string Subject, string Email, string? Name);

public interface IGoogleTokenValidator
{
    Task<GoogleTokenPayload> ValidateAsync(string? idToken, string? accessToken, CancellationToken cancellationToken = default);
}
