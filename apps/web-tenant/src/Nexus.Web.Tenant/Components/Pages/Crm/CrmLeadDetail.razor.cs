using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;

namespace Nexus.Web.Tenant.Components.Pages.Crm;

public partial class CrmLeadDetail
{
    [Parameter] public Guid Id { get; set; }

    private bool _loading;
    private LeadDto? _lead;
    private CustomerDto? _convertedCustomer;
    private OpportunityDto? _convertedOpportunity;
    private IReadOnlyList<ActivityDto> _activities = [];
    private Modal? _convertModal;
    private ConvertFormModel _convertModel = new();
    private string _customerTypeText = CustomerType.Company.ToString();
    private List<SelectedItem> _customerTypeOptions = CrmLabels.CustomerTypeOptions();

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        _loading = true;
        try
        {
            _lead = await CrmApi.GetLeadAsync(Id);
            _convertedCustomer = null;
            _convertedOpportunity = null;
            _activities = [];
            if (_lead is not null)
            {
                if (_lead.ConvertedCustomerId.HasValue)
                {
                    _convertedCustomer = await CrmApi.GetCustomerAsync(_lead.ConvertedCustomerId.Value);
                }

                if (_lead.ConvertedOpportunityId.HasValue)
                {
                    _convertedOpportunity = await CrmApi.GetOpportunityAsync(_lead.ConvertedOpportunityId.Value);
                }

                _activities = (await CrmApi.GetActivitiesAsync(new ActivityListQuery
                {
                    RelatedEntityType = CrmRelatedEntityType.Lead,
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

    private Task ShowConvertModalAsync()
    {
        _convertModel = new ConvertFormModel
        {
            CustomerCode = BuildDefaultCustomerCode(_lead!.FullName),
            OpportunityName = $"Cơ hội - {_lead.CompanyName ?? _lead.FullName}",
            OpportunityAmount = 0
        };
        return _convertModal!.Show();
    }

    private Task CloseConvertModalAsync() => _convertModal!.Close();

    private async Task ConvertAsync()
    {
        if (_lead is null || string.IsNullOrWhiteSpace(_convertModel.CustomerCode) || string.IsNullOrWhiteSpace(_convertModel.OpportunityName))
        {
            await ShowErrorAsync(new InvalidOperationException("Vui lòng nhập mã khách hàng và tên cơ hội."));
            return;
        }

        if (!Enum.TryParse<CustomerType>(_customerTypeText, out var customerType))
        {
            customerType = CustomerType.Company;
        }

        try
        {
            var result = await CrmApi.ConvertLeadAsync(_lead.Id, new ConvertLeadRequest(
                _convertModel.CustomerCode.Trim(),
                customerType,
                _convertModel.OpportunityName.Trim(),
                _convertModel.OpportunityAmount,
                null,
                null));

            await _convertModal!.Close();
            await ToastService.Success("Thành công", "Đã chuyển đổi lead thành khách hàng và cơ hội.");
            if (result?.CustomerId is Guid customerId)
            {
                Navigation.NavigateTo($"crm/customers/{customerId}");
            }
            else
            {
                await LoadAsync();
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private void GoBack() => Navigation.NavigateTo("crm/leads");
    private void OpenCustomer(Guid id) => Navigation.NavigateTo($"crm/customers/{id}");
    private void OpenOpportunity(Guid id) => Navigation.NavigateTo($"crm/opportunities/{id}");

    private static string BuildDefaultCustomerCode(string fullName)
    {
        var prefix = new string(fullName.Where(char.IsLetterOrDigit).Take(6).ToArray()).ToUpperInvariant();
        return string.IsNullOrWhiteSpace(prefix) ? $"KH-{DateTime.Now:yyyyMMdd}" : $"KH-{prefix}";
    }

    private sealed class ConvertFormModel
    {
        public string CustomerCode { get; set; } = "";
        public string OpportunityName { get; set; } = "";
        public decimal OpportunityAmount { get; set; }
    }
}
