using BootstrapBlazor.Components;

namespace Nexus.Web.Tenant.Components.Pages.Crm;

public partial class CrmCustomers
{
    private Table<CustomerDto>? _table;
    private Modal? _editModal;
    private bool _isCreate;
    private string _modalTitle = "";
    private CustomerDto? _editing;
    private CustomerFormModel _createModel = new();
    private CustomerFormModel _editModel = new();
    // Default to the first customer type so the form is valid out of the box.
    private string _customerTypeText = CrmLabels.DefaultCustomerType();
    private string _customerStatusText = CustomerStatus.Active.ToString();
    private List<SelectedItem> _customerTypeOptions = CrmLabels.CustomerTypeOptions();
    private List<SelectedItem> _customerStatusOptions = CrmLabels.CustomerStatusOptions();
    private Dictionary<string, (string Text, string BadgeClass)> _customerStatusMeta = CrmLabels.CustomerStatusMeta();

    private async Task<QueryData<CustomerDto>> OnQueryAsync(QueryPageOptions options)
    {
        try
        {
            var result = await CrmApi.GetCustomersAsync(new CustomerListQuery
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
            return EmptyQuery<CustomerDto>();
        }
    }

    private Task ShowCreateAsync(IEnumerable<CustomerDto> _)
    {
        _isCreate = true;
        _modalTitle = "Thêm khách hàng";
        _createModel = new CustomerFormModel();
        _editModel = new CustomerFormModel();
        _customerTypeText = CrmLabels.DefaultCustomerType();
        return _editModal!.Show();
    }

    private Task ShowEditAsync(CustomerDto customer)
    {
        _isCreate = false;
        _modalTitle = "Sửa khách hàng";
        _editing = customer;
        _editModel = new CustomerFormModel
        {
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            Industry = customer.Industry,
            City = customer.City,
            Website = customer.Website,
            TaxCode = customer.TaxCode,
            AddressLine1 = customer.AddressLine1,
            Description = customer.Description,
            Source = customer.Source
        };
        _customerTypeText = customer.CustomerType.ToString();
        _customerStatusText = customer.Status.ToString();
        return _editModal!.Show();
    }

    private Task CloseModalAsync() => _editModal!.Close();

    private async Task SaveAsync()
    {
        try
        {
            if (!Enum.TryParse<CustomerType>(_customerTypeText, out var customerType))
            {
                await ShowErrorAsync(new InvalidOperationException("Vui lòng chọn loại khách hàng."));
                return;
            }

            if (_isCreate)
            {
                if (string.IsNullOrWhiteSpace(_createModel.Code) || string.IsNullOrWhiteSpace(_editModel.Name))
                {
                    await ShowErrorAsync(new InvalidOperationException("Vui lòng nhập mã và tên khách hàng."));
                    return;
                }

                await CrmApi.CreateCustomerAsync(new CreateCustomerRequest(
                    _createModel.Code.Trim(),
                    _editModel.Name.Trim(),
                    customerType,
                    _editModel.Email,
                    _editModel.Phone,
                    _editModel.Industry,
                    _editModel.City,
                    _editModel.Source));
                await ToastService.Success("Thành công", "Đã tạo khách hàng.");
            }
            else if (_editing is not null)
            {
                if (!Enum.TryParse<CustomerStatus>(_customerStatusText, out var status))
                {
                    status = _editing.Status;
                }

                await CrmApi.UpdateCustomerAsync(_editing.Id, new UpdateCustomerRequest(
                    _editModel.Name.Trim(),
                    customerType,
                    _editModel.Email,
                    _editModel.Phone,
                    _editModel.TaxCode,
                    _editModel.Website,
                    _editModel.Industry,
                    _editModel.AddressLine1,
                    null,
                    _editModel.City,
                    null,
                    null,
                    null,
                    null,
                    _editModel.Description,
                    null,
                    _editModel.Source,
                    status));
                await ToastService.Success("Thành công", "Đã cập nhật khách hàng.");
            }

            await _editModal!.Close();
            await ReloadTableAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private void ViewDetailAsync(Guid id) => Navigation.NavigateTo($"crm/customers/{id}");

    private async Task DeleteAsync(Guid id)
    {
        if (!await ConfirmDeleteAsync())
        {
            return;
        }

        try
        {
            await CrmApi.DeleteCustomerAsync(id);
            await ToastService.Success("Đã xoá", "Khách hàng đã được xoá.");
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

    private sealed class CustomerFormModel
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Industry { get; set; }
        public string? City { get; set; }
        public string? Website { get; set; }
        public string? TaxCode { get; set; }
        public string? AddressLine1 { get; set; }
        public string? Description { get; set; }
        public string? Source { get; set; }
    }
}
