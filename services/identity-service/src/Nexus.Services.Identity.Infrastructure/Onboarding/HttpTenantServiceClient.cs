using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Nexus.Services.Identity.Application.Onboarding;
using Nexus.Services.Tenant.Contracts.Tenants;

namespace Nexus.Services.Identity.Infrastructure.Onboarding;

public sealed class HttpTenantServiceClient : ITenantServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly string _internalApiKey;

    public HttpTenantServiceClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _internalApiKey = configuration["Internal:ApiKey"] ?? "nexus-internal-dev-key";
        var baseUrl = configuration["Services:Tenant"] ?? "http://localhost:7201";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<bool> IsCodeAvailableAsync(string code, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/api/public/tenants/code-available/{Uri.EscapeDataString(code.Trim())}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CodeAvailabilityResponse>(cancellationToken);
        return result?.Available ?? false;
    }

    public async Task<TenantDto> CreateTenantAsync(CreateInternalTenantDto input, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/internal/tenants");
        request.Headers.Add("X-Internal-Api-Key", _internalApiKey);
        request.Content = JsonContent.Create(input);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Failed to create tenant: {response.StatusCode} {body}");
        }

        return (await response.Content.ReadFromJsonAsync<TenantDto>(cancellationToken))!;
    }

    public async Task<TenantDto?> GetTenantByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/api/public/tenants/by-id/{tenantId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TenantDto>(cancellationToken);
    }

    private sealed record CodeAvailabilityResponse(string Code, bool Available);
}
