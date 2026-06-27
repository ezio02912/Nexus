using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Nexus.Services.Crm.Contracts.Numbering;

namespace Nexus.Services.Crm.Infrastructure.Numbering;

public sealed class HttpNumberingClient : INumberingClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public HttpNumberingClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["Services:Numbering"] ?? "http://localhost:7206";
    }

    public async Task<string> GetNextNumberAsync(Guid tenantId, string module, string documentType, string prefix, CancellationToken cancellationToken = default)
    {
        var url = $"{_baseUrl.TrimEnd('/')}/api/numbering/next";
        var response = await _httpClient.PostAsJsonAsync(url, new
        {
            TenantId = tenantId,
            Module = module,
            DocumentType = documentType,
            Prefix = prefix,
            Padding = 5
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            // Fallback when numbering service is unavailable
            return $"{prefix}{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        }

        var result = await response.Content.ReadFromJsonAsync<NextNumberResponse>(cancellationToken);
        return result?.Number ?? $"{prefix}00001";
    }

    private sealed record NextNumberResponse(string SequenceKey, string Number, long Value);
}
