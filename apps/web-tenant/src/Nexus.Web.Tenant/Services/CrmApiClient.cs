using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Nexus.Web.Tenant.Services;

public sealed class CrmApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TenantPortalOptions _options;
    private readonly TenantSessionService _session;

    public CrmApiClient(
        IHttpClientFactory httpClientFactory,
        IOptions<TenantPortalOptions> options,
        TenantSessionService session)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _session = session;
    }

    public Task<PagedResultDto<CustomerDto>?> GetCustomersAsync(CustomerListQuery? query = null)
        => GetAsync<PagedResultDto<CustomerDto>>("/api/crm/customers", BuildQuery(query));

    public Task<CustomerDto?> GetCustomerAsync(Guid id)
        => GetAsync<CustomerDto>($"/api/crm/customers/{id}");

    public Task<CustomerDto?> CreateCustomerAsync(CreateCustomerRequest request)
        => PostAsync<CustomerDto>("/api/crm/customers", request);

    public Task<CustomerDto?> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request)
        => PutAsync<CustomerDto>($"/api/crm/customers/{id}", request);

    public Task DeleteCustomerAsync(Guid id)
        => DeleteAsync($"/api/crm/customers/{id}");

    public Task<PagedResultDto<ContactDto>?> GetContactsAsync(ContactListQuery? query = null)
        => GetAsync<PagedResultDto<ContactDto>>("/api/crm/contacts", BuildQuery(query));

    public Task<ContactDto?> GetContactAsync(Guid id)
        => GetAsync<ContactDto>($"/api/crm/contacts/{id}");

    public Task<ContactDto?> CreateContactAsync(CreateContactRequest request)
        => PostAsync<ContactDto>("/api/crm/contacts", request);

    public Task<ContactDto?> UpdateContactAsync(Guid id, UpdateContactRequest request)
        => PutAsync<ContactDto>($"/api/crm/contacts/{id}", request);

    public Task DeleteContactAsync(Guid id)
        => DeleteAsync($"/api/crm/contacts/{id}");

    public Task<PagedResultDto<LeadDto>?> GetLeadsAsync(LeadListQuery? query = null)
        => GetAsync<PagedResultDto<LeadDto>>("/api/crm/leads", BuildQuery(query));

    public Task<LeadDto?> GetLeadAsync(Guid id)
        => GetAsync<LeadDto>($"/api/crm/leads/{id}");

    public Task<LeadDto?> CreateLeadAsync(CreateLeadRequest request)
        => PostAsync<LeadDto>("/api/crm/leads", request);

    public Task<LeadDto?> UpdateLeadAsync(Guid id, UpdateLeadRequest request)
        => PutAsync<LeadDto>($"/api/crm/leads/{id}", request);

    public Task DeleteLeadAsync(Guid id)
        => DeleteAsync($"/api/crm/leads/{id}");

    public Task<ConvertLeadResultDto?> ConvertLeadAsync(Guid id, ConvertLeadRequest request)
        => PostAsync<ConvertLeadResultDto>($"/api/crm/leads/{id}/convert", request);

    public Task<PagedResultDto<OpportunityDto>?> GetOpportunitiesAsync(OpportunityListQuery? query = null)
        => GetAsync<PagedResultDto<OpportunityDto>>("/api/crm/opportunities", BuildQuery(query));

    public Task<OpportunityDto?> GetOpportunityAsync(Guid id)
        => GetAsync<OpportunityDto>($"/api/crm/opportunities/{id}");

    public Task<OpportunityDto?> CreateOpportunityAsync(CreateOpportunityRequest request)
        => PostAsync<OpportunityDto>("/api/crm/opportunities", request);

    public Task<OpportunityDto?> UpdateOpportunityAsync(Guid id, UpdateOpportunityRequest request)
        => PutAsync<OpportunityDto>($"/api/crm/opportunities/{id}", request);

    public Task DeleteOpportunityAsync(Guid id)
        => DeleteAsync($"/api/crm/opportunities/{id}");

    public Task<OpportunityDto?> ChangeOpportunityStageAsync(Guid id, ChangeOpportunityStageRequest request)
        => PatchAsync<OpportunityDto>($"/api/crm/opportunities/{id}/stage", request);

    public Task<PagedResultDto<QuotationDto>?> GetQuotationsAsync(QuotationListQuery? query = null)
        => GetAsync<PagedResultDto<QuotationDto>>("/api/crm/quotations", BuildQuery(query));

    public Task<QuotationDto?> GetQuotationAsync(Guid id)
        => GetAsync<QuotationDto>($"/api/crm/quotations/{id}");

    public Task<QuotationDto?> CreateQuotationAsync(CreateQuotationRequest request)
        => PostAsync<QuotationDto>("/api/crm/quotations", request);

    public Task<QuotationDto?> UpdateQuotationAsync(Guid id, UpdateQuotationRequest request)
        => PutAsync<QuotationDto>($"/api/crm/quotations/{id}", request);

    public Task DeleteQuotationAsync(Guid id)
        => DeleteAsync($"/api/crm/quotations/{id}");

    public Task<QuotationDto?> ApproveQuotationAsync(Guid id)
        => PostAsync<QuotationDto>($"/api/crm/quotations/{id}/approve", new { });

    public Task<QuotationDto?> RejectQuotationAsync(Guid id, RejectQuotationRequest request)
        => PostAsync<QuotationDto>($"/api/crm/quotations/{id}/reject", request);

    public Task<QuotationDto?> SendQuotationAsync(Guid id)
        => PostAsync<QuotationDto>($"/api/crm/quotations/{id}/send", new { });

    public Task<PagedResultDto<ContractDto>?> GetContractsAsync(ContractListQuery? query = null)
        => GetAsync<PagedResultDto<ContractDto>>("/api/crm/contracts", BuildQuery(query));

    public Task<ContractDto?> GetContractAsync(Guid id)
        => GetAsync<ContractDto>($"/api/crm/contracts/{id}");

    public Task<ContractDto?> CreateContractAsync(CreateContractRequest request)
        => PostAsync<ContractDto>("/api/crm/contracts", request);

    public Task<ContractDto?> UpdateContractAsync(Guid id, UpdateContractRequest request)
        => PutAsync<ContractDto>($"/api/crm/contracts/{id}", request);

    public Task DeleteContractAsync(Guid id)
        => DeleteAsync($"/api/crm/contracts/{id}");

    public Task<ContractDto?> SignContractAsync(Guid id)
        => PostAsync<ContractDto>($"/api/crm/contracts/{id}/sign", new { });

    public Task<ContractDto?> ActivateContractAsync(Guid id)
        => PostAsync<ContractDto>($"/api/crm/contracts/{id}/activate", new { });

    public Task<ContractDto?> CompleteContractAsync(Guid id)
        => PostAsync<ContractDto>($"/api/crm/contracts/{id}/complete", new { });

    public Task<ContractDto?> TerminateContractAsync(Guid id, TerminateContractRequest request)
        => PostAsync<ContractDto>($"/api/crm/contracts/{id}/terminate", request);

    public Task<PagedResultDto<ActivityDto>?> GetActivitiesAsync(ActivityListQuery? query = null)
        => GetAsync<PagedResultDto<ActivityDto>>("/api/crm/activities", BuildQuery(query));

    public Task<ActivityDto?> GetActivityAsync(Guid id)
        => GetAsync<ActivityDto>($"/api/crm/activities/{id}");

    public Task<ActivityDto?> CreateActivityAsync(CreateActivityRequest request)
        => PostAsync<ActivityDto>("/api/crm/activities", request);

    public Task<ActivityDto?> UpdateActivityAsync(Guid id, UpdateActivityRequest request)
        => PutAsync<ActivityDto>($"/api/crm/activities/{id}", request);

    public Task DeleteActivityAsync(Guid id)
        => DeleteAsync($"/api/crm/activities/{id}");

    public Task<ActivityDto?> CompleteActivityAsync(Guid id)
        => PostAsync<ActivityDto>($"/api/crm/activities/{id}/complete", new { });

    public Task<CrmDashboardDto?> GetDashboardAsync()
        => GetAsync<CrmDashboardDto>("/api/crm/dashboard");

    private async Task<T?> GetAsync<T>(string path, string? queryString = null)
    {
        var url = BuildUrl(path, queryString);
        var response = await CreateClient().GetAsync(url);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }

        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    private async Task<T?> PostAsync<T>(string path, object request)
    {
        var response = await CreateClient().PostAsJsonAsync(BuildUrl(path), request, JsonOptions);
        await EnsureSuccessAsync(response);
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    private async Task<T?> PutAsync<T>(string path, object request)
    {
        var response = await CreateClient().PutAsJsonAsync(BuildUrl(path), request, JsonOptions);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    private async Task<T?> PatchAsync<T>(string path, object request)
    {
        var json = JsonSerializer.Serialize(request, JsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await CreateClient().PatchAsync(BuildUrl(path), content);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    private async Task DeleteAsync(string path)
    {
        var response = await CreateClient().DeleteAsync(BuildUrl(path));
        await EnsureSuccessAsync(response);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        throw new InvalidOperationException(MasterDataApiClient.FormatApiError(response.StatusCode, body, "crm"));
    }

    private string BuildUrl(string path, string? queryString = null)
    {
        var baseUrl = _options.Crm.TrimEnd('/');
        return string.IsNullOrEmpty(queryString) ? $"{baseUrl}{path}" : $"{baseUrl}{path}?{queryString}";
    }

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

    private static string BuildQuery(object? query)
    {
        if (query is null)
        {
            return string.Empty;
        }

        var parts = new List<string>();
        foreach (var property in query.GetType().GetProperties())
        {
            var value = property.GetValue(query);
            if (value is null)
            {
                continue;
            }

            var name = char.ToLowerInvariant(property.Name[0]) + property.Name[1..];
            parts.Add($"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(FormatQueryValue(value))}");
        }

        return string.Join('&', parts);
    }

    private static string FormatQueryValue(object value) => value switch
    {
        DateOnly date => date.ToString("O"),
        DateTimeOffset dto => dto.ToString("O"),
        Enum enumValue => enumValue.ToString()!,
        _ => Convert.ToString(value) ?? string.Empty
    };
}
