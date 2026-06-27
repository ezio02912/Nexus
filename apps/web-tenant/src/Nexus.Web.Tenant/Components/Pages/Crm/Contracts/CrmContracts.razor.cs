using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;

namespace Nexus.Web.Tenant.Components.Pages.Crm.Contracts;

public partial class CrmContracts
{
    private Table<ContractDto>? _table;
    private Modal? _editModal;
    private ContractFormModel _model = new();
    private string _customerIdText = "";
    private string? _contactIdText;
    private List<SelectedItem> _customerOptions = [];
    private Dictionary<Guid, string> _customerNames = [];

    [Inject] private FileApiClient FileApi { get; set; } = default!;

    protected override async Task OnInitializedAsync() => await LoadCustomersAsync();

    private async Task LoadCustomersAsync()
    {
        try
        {
            var result = await CrmApi.GetCustomersAsync(new CustomerListQuery { MaxResultCount = 500 });
            _customerNames = (result?.Items ?? []).ToDictionary(x => x.Id, x => $"{x.Code} - {x.Name}");
            _customerOptions = _customerNames.Select(x => new SelectedItem(x.Key.ToString(), x.Value)).ToList();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private string CustomerName(Guid customerId) =>
        _customerNames.TryGetValue(customerId, out var name) ? name : customerId.ToString();

    private void ViewCustomerDetail(Guid id) => Navigation.NavigateTo($"crm/customers/{id}");
    private void ViewOpportunityDetail(Guid id) => Navigation.NavigateTo($"crm/opportunities/{id}");
    private void ViewQuotationDetail(Guid id) => Navigation.NavigateTo($"crm/quotations/{id}");

    private async Task<QueryData<ContractDto>> OnQueryAsync(QueryPageOptions options)
    {
        try
        {
            var result = await CrmApi.GetContractsAsync(new ContractListQuery
            {
                Search = options.SearchText,
                SkipCount = (options.PageIndex - 1) * options.PageItems,
                MaxResultCount = options.PageItems,
                Sorting = options.SortName
            });
            return ToQueryData(result, options);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
            return EmptyQuery<ContractDto>();
        }
    }

    private Task ShowCreateAsync(IEnumerable<ContractDto> _)
    {
        _model = new ContractFormModel
        {
            ContractNo = "AUTO",
            ContractValue = 0,
            Currency = "VND",
            StartDate = DateTime.Today
        };
        _customerIdText = _customerOptions.FirstOrDefault()?.Value ?? "";
        _contactIdText = null;
        return _editModal!.Show();
    }

    private Task CloseModalAsync() => _editModal!.Close();

    private async Task SaveAsync()
    {
        if (!Guid.TryParse(_customerIdText, out var customerId)
            || string.IsNullOrWhiteSpace(_model.ContractNo)
            || string.IsNullOrWhiteSpace(_model.Title))
        {
            await ShowErrorAsync(new InvalidOperationException("Vui lòng chọn khách hàng, nhập số và tiêu đề hợp đồng."));
            return;
        }

        // A contract file is mandatory when creating a contract.
        if (_model.PendingFiles.Count == 0)
        {
            await ShowErrorAsync(new InvalidOperationException("Vui lòng đính kèm tệp hợp đồng trước khi lưu."));
            return;
        }

        try
        {
            Guid? contactId = Guid.TryParse(_contactIdText, out var parsedContactId) ? parsedContactId : null;
            var startDate = ToDateOnly(_model.StartDate);
            var endDate = ToDateOnly(_model.EndDate);
            var renewalDate = ToDateOnly(_model.RenewalDate);
            var contract = await CrmApi.CreateContractAsync(new CreateContractRequest(
                customerId, _model.ContractNo.Trim(), _model.Title.Trim(),
                null, null, contactId, _model.ContractValue,
                string.IsNullOrWhiteSpace(_model.Currency) ? "VND" : _model.Currency.Trim(),
                startDate, endDate, renewalDate,
                _model.PaymentTerms, _model.Notes, _model.Terms, null, null, []));
            if (contract is not null && _model.PendingFiles.Count > 0)
            {
                await FileApi.UploadAndLinkAsync(_model.PendingFiles, "CRM", "Contract", contract.Id.ToString(), DocumentFileCatalog.Contract);
            }

            await ToastService.Success("Thành công", "Đã tạo hợp đồng.");
            await _editModal!.Close();
            await ReloadTableAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private async Task SignAsync(Guid id)
    {
        try
        {
            await CrmApi.SignContractAsync(id);
            await ToastService.Success("Thành công", "Đã ký hợp đồng.");
            await ReloadTableAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private void ViewDetailAsync(Guid id) => Navigation.NavigateTo($"crm/contracts/{id}");

    private async Task DeleteAsync(Guid id)
    {
        if (!await ConfirmDeleteAsync())
        {
            return;
        }

        try
        {
            await CrmApi.DeleteContractAsync(id);
            await ToastService.Success("Đã xoá", "Hợp đồng đã được xoá.");
            await ReloadTableAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, "Xoá thất bại");
        }
    }

    private async Task ReloadTableAsync()
    {
        if (_table is not null)
        {
            await _table.QueryAsync();
        }
    }

    private sealed class ContractFormModel
    {
        public string ContractNo { get; set; } = "";
        public string Title { get; set; } = "";
        public decimal ContractValue { get; set; }
        public string Currency { get; set; } = "VND";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? RenewalDate { get; set; }
        public string? PaymentTerms { get; set; }
        public string? Notes { get; set; }
        public string? Terms { get; set; }
        public List<PendingFileAttachment> PendingFiles { get; set; } = [];
    }

    private static DateOnly? ToDateOnly(DateTime? value) =>
        value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
}
