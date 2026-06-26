using System.Net.Http.Headers;
using System.Net.Http.Json;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Nexus.Services.Identity.Application.Onboarding;

namespace Nexus.Services.Identity.Infrastructure.Onboarding;

public sealed class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly string? _clientId;
    private readonly IHttpClientFactory _httpClientFactory;

    public GoogleTokenValidator(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _clientId = configuration["Google:ClientId"];
        _httpClientFactory = httpClientFactory;
    }

    public async Task<GoogleTokenPayload> ValidateAsync(string? idToken, string? accessToken, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(idToken))
        {
            return await ValidateIdTokenAsync(idToken, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            return await ValidateAccessTokenAsync(accessToken, cancellationToken);
        }

        throw new InvalidOperationException("Google ID token or access token is required.");
    }

    private async Task<GoogleTokenPayload> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = string.IsNullOrWhiteSpace(_clientId) ? null : [_clientId]
        };

        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        return MapPayload(payload.Subject, payload.Email, payload.Name);
    }

    private async Task<GoogleTokenPayload> ValidateAccessTokenAsync(string accessToken, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v3/userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Google access token is invalid.");
        }

        var profile = await response.Content.ReadFromJsonAsync<GoogleUserInfoResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Google user profile is empty.");

        return MapPayload(profile.Sub, profile.Email, profile.Name);
    }

    private static GoogleTokenPayload MapPayload(string? subject, string? email, string? name)
    {
        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("Google token does not contain required claims.");
        }

        return new GoogleTokenPayload(subject, email, name);
    }

    private sealed record GoogleUserInfoResponse(string Sub, string Email, string? Name);
}
