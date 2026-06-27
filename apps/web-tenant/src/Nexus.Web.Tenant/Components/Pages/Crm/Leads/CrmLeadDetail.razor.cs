using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;

namespace Nexus.Web.Tenant.Components.Pages.Crm.Leads;

public partial class CrmLeadDetail
{
    [Parameter] public Guid Id { get; set; }

    private bool _loading;
    private LeadDto? _lead;
    private CustomerDto? _convertedCustomer;
    private OpportunityDto? _convertedOpportunity;
    private IReadOnlyList<ActivityDto> _activities = [];
    private Modal? _editModal;
    private LeadEditModel _editModel = new();
    private string _leadStatusText = LeadStatus.New.ToString();
    private List<SelectedItem> _leadStatusOptions = CrmLabels.LeadStatusOptions();
    private Dictionary<string, (string Text, string BadgeClass)> _leadStatusMeta = CrmLabels.LeadStatusMeta();
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

    private Task ShowEditModalAsync()
    {
        if (_lead is null)
        {
            return Task.CompletedTask;
        }

        _leadStatusText = _lead.Status.ToString();
        _editModel = new LeadEditModel
        {
            FullName = _lead.FullName,
            CompanyName = _lead.CompanyName,
            Title = _lead.Title,
            Email = _lead.Email,
            Phone = _lead.Phone,
            Source = _lead.Source,
            LeadScore = _lead.LeadScore,
            Description = _lead.Description
        };
        return _editModal!.Show();
    }

    private Task CloseEditModalAsync() => _editModal!.Close();

    private async Task SaveEditAsync()
    {
        if (_lead is null || string.IsNullOrWhiteSpace(_editModel.FullName))
        {
            await ShowErrorAsync(new InvalidOperationException("Vui lòng nhập họ tên lead."));
            return;
        }

        if (!Enum.TryParse<LeadStatus>(_leadStatusText, out var status))
        {
            status = _lead.Status;
        }

        try
        {
            _lead = await CrmApi.UpdateLeadAsync(_lead.Id, new UpdateLeadRequest(
                _editModel.FullName.Trim(),
                _editModel.CompanyName,
                _editModel.Title,
                _editModel.Email,
                _editModel.Phone,
                _editModel.Source,
                _editModel.LeadScore,
                _lead.Rating,
                status,
                _lead.OwnerId,
                _editModel.Description,
                _lead.Address,
                _lead.City,
                _lead.Country,
                _lead.LostReason));

            await _editModal!.Close();
            await ToastService.Success("Thành công", "Đã cập nhật lead.");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

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

    private sealed class LeadEditModel
    {
        public string FullName { get; set; } = "";
        public string? CompanyName { get; set; }
        public string? Title { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Source { get; set; }
        public int LeadScore { get; set; }
        public string? Description { get; set; }
    }
}
