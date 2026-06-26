using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Nexus.Web.Tenant.Services;

public sealed class MasterDataApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TenantPortalOptions _options;
    private readonly TenantSessionService _session;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public MasterDataApiClient(
        IHttpClientFactory httpClientFactory,
        IOptions<TenantPortalOptions> options,
        TenantSessionService session)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _session = session;
    }

    public Task<PagedLookupItemsDto?> GetItemsAsync(LookupListQuery query)
    {
        var qs = $"category={Uri.EscapeDataString(query.Category)}&skipCount={query.SkipCount}&maxResultCount={query.MaxResultCount}";
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            qs += $"&search={Uri.EscapeDataString(query.Search)}";
        }

        return GetAsync<PagedLookupItemsDto>($"/api/master-data/items?{qs}");
    }

    private async Task<T?> GetAsync<T>(string path)
    {
        var response = await CreateClient().GetAsync(BuildUrl(path));
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }

        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        throw new InvalidOperationException(FormatApiError(response.StatusCode, body, "master data"));
    }

    internal static string FormatApiError(System.Net.HttpStatusCode statusCode, string body, string serviceName)
    {
        if ((int)statusCode == 404)
        {
            return serviceName switch
            {
                "crm" => "Không tìm thấy dịch vụ CRM (404). Hãy chạy Nexus.Services.Crm.Api trên cổng 7208 — hiện cổng này có thể đang bị Notification Service chiếm.",
                "master data" => "Không tìm thấy dịch vụ Master Data (404). Hãy chạy Nexus.Services.MasterData.Api trên cổng 7211.",
                _ => $"Yêu cầu {serviceName} thất bại (404)."
            };
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return $"Yêu cầu {serviceName} thất bại ({(int)statusCode}).";
        }

        return body;
    }

    private string BuildUrl(string path)
    {
        var baseUrl = _options.MasterData.TrimEnd('/');
        return $"{baseUrl}{path}";
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient();
        if (_session.IsAuthenticated && !string.IsNullOrEmpty(_session.Login?.AccessToken))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _session.Login.AccessToken);
        }

        return client;
    }
}
