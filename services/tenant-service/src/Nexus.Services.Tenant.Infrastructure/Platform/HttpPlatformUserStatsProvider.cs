using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Nexus.Services.Tenant.Contracts.Platform;
using Nexus.Services.Tenant.Contracts.Subscriptions;

namespace Nexus.Services.Tenant.Infrastructure.Platform;

public sealed class HttpPlatformUserStatsProvider : IPlatformUserStatsProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _internalApiKey;

    public HttpPlatformUserStatsProvider(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _internalApiKey = configuration["Internal:ApiKey"] ?? "nexus-internal-dev-key";
        var baseUrl = configuration["Services:Identity"] ?? "http://localhost:7202";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<PlatformUserStatsDto> GetUserStatsAsync(CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/internal/platform/user-stats");
        request.Headers.Add("X-Internal-Api-Key", _internalApiKey);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new PlatformUserStatsDto();
        }

        return await response.Content.ReadFromJsonAsync<PlatformUserStatsDto>(cancellationToken)
            ?? new PlatformUserStatsDto();
    }
}
