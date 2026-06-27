using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Nexus.Web.Tenant.Services;

/// <summary>
/// Talks to the file-service: binary upload (multipart), linking a file to a business
/// entity (module / entity type / entity id) and listing/downloading attachments.
/// </summary>
public sealed class FileApiClient
{
    private const long MaxFileSize = 20 * 1024 * 1024;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TenantPortalOptions _options;
    private readonly TenantSessionService _session;

    public FileApiClient(
        IHttpClientFactory httpClientFactory,
        IOptions<TenantPortalOptions> options,
        TenantSessionService session)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _session = session;
    }

    public async Task<FileDto?> UploadAsync(Stream content, string fileName, string? contentType)
    {
        var client = CreateClient();
        using var form = new MultipartFormDataContent();
        var streamContent = new StreamContent(content);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
        form.Add(streamContent, "file", fileName);

        var url = $"{BaseUrl}/api/files/upload";
        if (_session.TenantId is Guid tenantId)
        {
            url += $"?tenantId={tenantId}";
        }

        var response = await client.PostAsync(url, form);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FileDto>(JsonOptions);
    }

    public async Task LinkAsync(Guid fileId, string module, string entityType, string entityId, string? category = null)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync(
            $"{BaseUrl}/api/file-links",
            new CreateFileLinkRequest(fileId, module, entityType, entityId, category),
            JsonOptions);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<FileLinkRecord>> GetLinksAsync(string module, string entityType, string entityId, string? category = null)
    {
        var client = CreateClient();
        var url = $"{BaseUrl}/api/file-links" +
                  $"?module={Uri.EscapeDataString(module)}" +
                  $"&entityType={Uri.EscapeDataString(entityType)}" +
                  $"&entityId={Uri.EscapeDataString(entityId)}";
        if (!string.IsNullOrWhiteSpace(category))
        {
            url += $"&category={Uri.EscapeDataString(category)}";
        }

        var result = await client.GetFromJsonAsync<IReadOnlyList<FileLinkRecord>>(url, JsonOptions);
        return result ?? [];
    }

    public async Task<byte[]> DownloadAsync(Guid fileId)
    {
        var client = CreateClient();
        return await client.GetByteArrayAsync($"{BaseUrl}/api/files/{fileId}/content");
    }

    public async Task DeleteAsync(Guid fileId)
    {
        var client = CreateClient();
        var response = await client.DeleteAsync($"{BaseUrl}/api/files/{fileId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task UploadAndLinkAsync(IEnumerable<PendingFileAttachment> files, string module, string entityType, string entityId, string? category = null)
    {
        foreach (var attachment in files)
        {
            using var stream = new MemoryStream(attachment.Content);
            var uploaded = await UploadAsync(stream, attachment.FileName, attachment.ContentType);
            if (uploaded is not null)
            {
                await LinkAsync(uploaded.Id, module, entityType, entityId, category);
            }
        }
    }

    private string BaseUrl => _options.File.TrimEnd('/');

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient();
        if (_session.IsAuthenticated && !string.IsNullOrEmpty(_session.Login?.AccessToken))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _session.Login.AccessToken);
        }

        if (_session.TenantId is Guid tenantId)
        {
            client.DefaultRequestHeaders.Remove("x-tenant-id");
            client.DefaultRequestHeaders.Add("x-tenant-id", tenantId.ToString());
        }

        return client;
    }
}
