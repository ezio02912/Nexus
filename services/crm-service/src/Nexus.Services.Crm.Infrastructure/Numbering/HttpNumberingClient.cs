using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Nexus.Services.Crm.Contracts.Numbering;

namespace Nexus.Services.Crm.Infrastructure.Numbering;

public sealed class HttpNumberingClient : INumberingClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _baseUrl;

    public HttpNumberingClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _baseUrl = configuration["Services:Numbering"] ?? "http://localhost:7206";
    }

    public async Task<string> GetNextNumberAsync(Guid tenantId, string module, string documentType, string prefix, CancellationToken cancellationToken = default)
    {
        var url = $"{_baseUrl.TrimEnd('/')}/api/numbering/next";
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(new
            {
                TenantId = tenantId,
                Module = module,
                DocumentType = documentType,
                Prefix = prefix,
                Padding = 5
            })
        };

        // Forward the caller's bearer token so the numbering-service authorizes the request.
        var authorization = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authorization))
        {
            request.Headers.TryAddWithoutValidation("Authorization", authorization);
        }

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            // Fallback when numbering service is unavailable
            return $"{prefix}{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        }

        var result = await response.Content.ReadFromJsonAsync<NextNumberResponse>(cancellationToken);
        return result?.Number ?? $"{prefix}{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
    }

    private sealed record NextNumberResponse(string SequenceKey, string Number, long Value);
}
