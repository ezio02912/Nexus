using BootstrapBlazor.Components;

namespace Nexus.Web.Tenant.Components.Pages.Crm;

public partial class CrmContracts
{
    private Table<ContractDto>? _table;
    private Modal? _editModal;
    private ContractFormModel _model = new();
    private string _customerIdText = "";
    private List<SelectedItem> _customerOptions = [];
    private Dictionary<Guid, string> _customerNames = [];

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
            ContractNo = $"CT-{DateTime.Now:yyyyMMddHHmm}",
            ContractValue = 0
        };
        _customerIdText = _customerOptions.FirstOrDefault()?.Value ?? "";
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

        try
        {
            await CrmApi.CreateContractAsync(new CreateContractRequest(
                customerId, _model.ContractNo.Trim(), _model.Title.Trim(),
                null, null, null, _model.ContractValue, null, []));
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
    }
}
