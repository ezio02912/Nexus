using Microsoft.AspNetCore.Components;

namespace Nexus.Web.Tenant.Components.Pages.Crm;

public partial class CrmCustomerDetail
{
    [Parameter] public Guid Id { get; set; }

    private bool _loading;
    private CustomerDto? _customer;
    private IReadOnlyList<ContactDto> _contacts = [];
    private IReadOnlyList<OpportunityDto> _opportunities = [];
    private IReadOnlyList<QuotationDto> _quotations = [];
    private IReadOnlyList<ContractDto> _contracts = [];
    private IReadOnlyList<ActivityDto> _activities = [];

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        _loading = true;
        try
        {
            _customer = await CrmApi.GetCustomerAsync(Id);
            if (_customer is not null)
            {
                _contacts = (await CrmApi.GetContactsAsync(new ContactListQuery { CustomerId = Id, MaxResultCount = 50 }))?.Items ?? [];
                _opportunities = (await CrmApi.GetOpportunitiesAsync(new OpportunityListQuery { CustomerId = Id, MaxResultCount = 50 }))?.Items ?? [];
                _quotations = (await CrmApi.GetQuotationsAsync(new QuotationListQuery { CustomerId = Id, MaxResultCount = 50 }))?.Items ?? [];
                _contracts = (await CrmApi.GetContractsAsync(new ContractListQuery { CustomerId = Id, MaxResultCount = 50 }))?.Items ?? [];
                _activities = (await CrmApi.GetActivitiesAsync(new ActivityListQuery
                {
                    RelatedEntityType = CrmRelatedEntityType.Customer,
                    RelatedEntityId = Id,
                    MaxResultCount = 20
                }))?.Items ?? [];
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
        finally
        {
            _loading = false;
        }
    }

    private void GoBack() => Navigation.NavigateTo("crm/customers");
    private void OpenOpportunity(Guid id) => Navigation.NavigateTo($"crm/opportunities/{id}");
    private void OpenQuotation(Guid id) => Navigation.NavigateTo($"crm/quotations/{id}");
    private void OpenContract(Guid id) => Navigation.NavigateTo($"crm/contracts/{id}");
}
