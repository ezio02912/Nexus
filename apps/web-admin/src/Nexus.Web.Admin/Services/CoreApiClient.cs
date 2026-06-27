using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Nexus.Web.Admin.Services;

public sealed class CoreApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CoreServiceOptions _options;
    private readonly AdminSessionService _session;

    public CoreApiClient(IHttpClientFactory httpClientFactory, IOptions<CoreServiceOptions> options, AdminSessionService session)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _session = session;
    }

    public Task<PagedResult<TenantDto>?> GetTenantsAsync() => GetAsync<PagedResult<TenantDto>>(_options.Tenant, "/api/tenants/");
    // Public (unauthenticated) lookup so the login screen can accept a tenant code instead of a GUID.
    public async Task<TenantDto?> GetTenantByCodeAsync(string code)
    {
        var client = CreateClient();
        var response = await client.GetAsync(BuildUrl(_options.Tenant, $"/api/public/tenants/by-code/{Uri.EscapeDataString(code.Trim())}"));
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TenantDto>();
    }
    public Task<TenantDto?> CreateTenantAsync(CreateTenantRequest request) => PostAsync<TenantDto>(_options.Tenant, "/api/tenants/", request);
    public Task DeleteTenantAsync(Guid id) => DeleteAsync(_options.Tenant, $"/api/tenants/{id}");
    public Task<TenantDto?> ActivateTenantAsync(Guid id) => PostAsync<TenantDto>(_options.Tenant, $"/api/tenants/{id}/activate", new { });
    public Task<TenantDto?> SuspendTenantAsync(Guid id) => PostAsync<TenantDto>(_options.Tenant, $"/api/tenants/{id}/suspend", new { });
    public Task<TenantDto?> EnableTenantModuleAsync(Guid id, ChangeTenantModuleRequest request) => PostAsync<TenantDto>(_options.Tenant, $"/api/tenants/{id}/modules/enable", request);
    public Task<TenantDto?> DisableTenantModuleAsync(Guid id, ChangeTenantModuleRequest request) => PostAsync<TenantDto>(_options.Tenant, $"/api/tenants/{id}/modules/disable", request);
    public Task<TenantDto?> UpdateTenantSettingsAsync(Guid id, UpdateTenantSettingsRequest request) => PutAsync<TenantDto>(_options.Tenant, $"/api/tenants/{id}/settings", request);
    public Task<TenantDto?> UpdateTenantProfileAsync(Guid id, UpdateTenantProfileRequest request) => PutAsync<TenantDto>(_options.Tenant, $"/api/tenants/{id}/profile", request);
    public Task<TenantDto?> ChangeTenantSubscriptionAsync(Guid id, ChangeTenantSubscriptionRequest request) => PostAsync<TenantDto>(_options.Tenant, $"/api/tenants/{id}/subscription/change", request);
    public Task<IReadOnlyList<SubscriptionPlanDto>?> GetSubscriptionPlansAsync() => GetAsync<IReadOnlyList<SubscriptionPlanDto>>(_options.Tenant, "/api/subscription-plans/");
    public Task<PlatformDashboardDto?> GetPlatformDashboardAsync() => GetAsync<PlatformDashboardDto>(_options.Tenant, "/api/platform/dashboard");
    public Task<IReadOnlyList<SubscriptionPaymentDto>?> GetPlatformBillingInvoicesAsync() => GetAsync<IReadOnlyList<SubscriptionPaymentDto>>(_options.Tenant, "/api/platform/billing/invoices");

    public Task<PagedResult<UserDto>?> GetUsersAsync() => GetAsync<PagedResult<UserDto>>(_options.Identity, "/api/users/");
    public Task<UserDto?> CreateUserAsync(CreateUserRequest request) => PostAsync<UserDto>(_options.Identity, "/api/users/", request);
    public Task DeleteUserAsync(Guid id) => DeleteAsync(_options.Identity, $"/api/users/{id}");
    public Task<LoginResult?> LoginAsync(LoginRequest request) => PostAsync<LoginResult>(_options.Identity, "/api/auth/login", request);

    public Task<IReadOnlyList<string>?> GetPermissionCatalogAsync() => GetAsync<IReadOnlyList<string>>(_options.Permission, "/api/permissions");
    public Task<RolePermissionDto?> GetRolePermissionsAsync(string roleName) => GetAsync<RolePermissionDto>(_options.Permission, $"/api/roles/{Uri.EscapeDataString(roleName)}/permissions");
    public Task<RolePermissionDto?> UpdateRolePermissionsAsync(string roleName, UpdateRolePermissionsRequest request) => PutAsync<RolePermissionDto>(_options.Permission, $"/api/roles/{Uri.EscapeDataString(roleName)}/permissions", request);

    public Task<PagedResult<AuditLogEntry>?> GetAuditLogsAsync() => GetAsync<PagedResult<AuditLogEntry>>(_options.Audit, "/api/audit-logs");
    public Task<AuditLogEntry?> CreateAuditLogAsync(CreateAuditLogRequest request) => PostAsync<AuditLogEntry>(_options.Audit, "/api/audit-logs", request);

    public Task<PagedResult<FileRecord>?> GetFilesAsync() => GetAsync<PagedResult<FileRecord>>(_options.File, "/api/files");
    public Task<FileRecord?> CreateFileAsync(CreateFileRequest request) => PostAsync<FileRecord>(_options.File, "/api/files", request);
    public Task DeleteFileAsync(Guid id) => DeleteAsync(_options.File, $"/api/files/{id}");
    public Task<FileLinkRecord?> CreateFileLinkAsync(CreateFileLinkRequest request) => PostAsync<FileLinkRecord>(_options.File, "/api/file-links", request);

    public Task<NextNumberResult?> GetNextNumberAsync(NextNumberRequest request) => PostAsync<NextNumberResult>(_options.Numbering, "/api/numbering/next", request);
    public Task<IReadOnlyList<NumberSequenceDto>?> GetNumberSequencesAsync() => GetAsync<IReadOnlyList<NumberSequenceDto>>(_options.Numbering, "/api/numbering/sequences");

    public Task<IReadOnlyList<WorkflowDefinitionRecord>?> GetWorkflowDefinitionsAsync() => GetAsync<IReadOnlyList<WorkflowDefinitionRecord>>(_options.Workflow, "/api/workflow-definitions");
    public Task<WorkflowDefinitionRecord?> CreateWorkflowDefinitionAsync(CreateWorkflowDefinitionRequest request) => PostAsync<WorkflowDefinitionRecord>(_options.Workflow, "/api/workflow-definitions", request);
    public Task DeleteWorkflowDefinitionAsync(Guid id) => DeleteAsync(_options.Workflow, $"/api/workflow-definitions/{id}");
    public Task<WorkflowInstanceRecord?> CreateWorkflowInstanceAsync(CreateWorkflowInstanceRequest request) => PostAsync<WorkflowInstanceRecord>(_options.Workflow, "/api/workflow-instances", request);

    public Task<IReadOnlyList<MasterDataCategoryDto>?> GetMasterDataCategoriesAsync()
        => GetAsync<IReadOnlyList<MasterDataCategoryDto>>(_options.MasterData, "/api/master-data/categories");

    public Task<IReadOnlyList<LookupItemDto>?> GetMasterDataItemsAsync(string? category = null)
    {
        var path = string.IsNullOrWhiteSpace(category)
            ? "/api/master-data/admin/items"
            : $"/api/master-data/admin/items?category={Uri.EscapeDataString(category)}";
        return GetAsync<IReadOnlyList<LookupItemDto>>(_options.MasterData, path);
    }

    public Task<LookupItemDto?> CreateMasterDataItemAsync(CreateLookupItemRequest request)
        => PostAsync<LookupItemDto>(_options.MasterData, "/api/master-data/admin/items", request);

    public Task<LookupItemDto?> UpdateMasterDataItemAsync(Guid id, UpdateLookupItemRequest request)
        => PutAsync<LookupItemDto>(_options.MasterData, $"/api/master-data/admin/items/{id}", request);

    public Task DeleteMasterDataItemAsync(Guid id) => DeleteAsync(_options.MasterData, $"/api/master-data/admin/items/{id}");

    private async Task<T?> GetAsync<T>(string baseUrl, string path)
    {
        var client = CreateClient();
        return await client.GetFromJsonAsync<T>(BuildUrl(baseUrl, path));
    }

    private async Task<T?> PostAsync<T>(string baseUrl, string path, object request)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync(BuildUrl(baseUrl, path), request);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>();
    }

    private async Task<T?> PutAsync<T>(string baseUrl, string path, object request)
    {
        var client = CreateClient();
        var response = await client.PutAsJsonAsync(BuildUrl(baseUrl, path), request);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>();
    }

    private async Task DeleteAsync(string baseUrl, string path)
    {
        var client = CreateClient();
        var response = await client.DeleteAsync(BuildUrl(baseUrl, path));
        await EnsureSuccessAsync(response);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        var message = ExtractErrorMessage(body);
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
            ? $"Yêu cầu thất bại ({(int)response.StatusCode})."
            : message);
    }

    private static string? ExtractErrorMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("message", out var message)
                || doc.RootElement.TryGetProperty("Message", out message)
                || doc.RootElement.TryGetProperty("error", out message))
            {
                return message.GetString();
            }
        }
        catch
        {
            return body;
        }

        return body;
    }

    // Routes go through the API gateway, so the per-service base already includes the gateway
    // prefix (e.g. http://localhost:7200/tenant). We compose the absolute URL ourselves rather
    // than relying on BaseAddress because the API paths start with '/'.
    private static string BuildUrl(string baseUrl, string path) => baseUrl.TrimEnd('/') + path;

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
