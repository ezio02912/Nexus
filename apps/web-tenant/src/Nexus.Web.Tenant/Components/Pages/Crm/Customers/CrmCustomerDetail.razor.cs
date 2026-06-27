using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;

namespace Nexus.Web.Tenant.Components.Pages.Crm.Customers;

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
    private Modal? _editModal;
    private CustomerEditModel _editModel = new();
    private string _customerTypeText = CustomerType.Company.ToString();
    private List<SelectedItem> _customerTypeOptions = CrmLabels.CustomerTypeOptions();
    private string _customerStatusText = CustomerStatus.Active.ToString();
    private List<SelectedItem> _customerStatusOptions = CrmLabels.CustomerStatusOptions();
    private Dictionary<string, (string Text, string BadgeClass)> _customerStatusMeta = CrmLabels.CustomerStatusMeta();

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

    private Task ShowEditModalAsync()
    {
        if (_customer is null)
        {
            return Task.CompletedTask;
        }

        _customerTypeText = _customer.CustomerType.ToString();
        _customerStatusText = _customer.Status.ToString();
        _editModel = new CustomerEditModel
        {
            Name = _customer.Name,
            Email = _customer.Email,
            Phone = _customer.Phone,
            TaxCode = _customer.TaxCode,
            Website = _customer.Website,
            Industry = _customer.Industry,
            City = _customer.City,
            Source = _customer.Source,
            AddressLine1 = _customer.AddressLine1,
            Description = _customer.Description
        };
        return _editModal!.Show();
    }

    private Task CloseEditModalAsync() => _editModal!.Close();

    private async Task SaveEditAsync()
    {
        if (_customer is null || string.IsNullOrWhiteSpace(_editModel.Name))
        {
            await ShowErrorAsync(new InvalidOperationException("Vui lòng nhập tên khách hàng."));
            return;
        }

        if (!Enum.TryParse<CustomerStatus>(_customerStatusText, out var status))
        {
            status = _customer.Status;
        }

        if (!Enum.TryParse<CustomerType>(_customerTypeText, out var customerType))
        {
            customerType = _customer.CustomerType;
        }

        try
        {
            _customer = await CrmApi.UpdateCustomerAsync(_customer.Id, new UpdateCustomerRequest(
                _editModel.Name.Trim(),
                customerType,
                _editModel.Email,
                _editModel.Phone,
                _editModel.TaxCode,
                _editModel.Website,
                _editModel.Industry,
                _editModel.AddressLine1,
                _customer.AddressLine2,
                _editModel.City,
                _customer.State,
                _customer.PostalCode,
                _customer.Country,
                _customer.OwnerId,
                _editModel.Description,
                _customer.Rating,
                _editModel.Source,
                status));

            await _editModal!.Close();
            await ToastService.Success("Thành công", "Đã cập nhật khách hàng.");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private sealed class CustomerEditModel
    {
        public string Name { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? TaxCode { get; set; }
        public string? Website { get; set; }
        public string? Industry { get; set; }
        public string? City { get; set; }
        public string? Source { get; set; }
        public string? AddressLine1 { get; set; }
        public string? Description { get; set; }
    }
}
