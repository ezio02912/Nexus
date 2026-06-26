using BootstrapBlazor.Components;

namespace Nexus.Web.Tenant.Components.Pages.Crm;

public partial class CrmQuotations
{
    private Table<QuotationDto>? _table;
    private Modal? _editModal;
    private bool _isCreate;
    private string _modalTitle = "";
    private QuotationDto? _editing;
    private QuotationFormModel _model = new();
    private string _customerIdText = "";
    private string? _contactIdText;
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

    private void ViewCustomerDetail(Guid id) => Navigation.NavigateTo($"crm/customers/{id}");
    private void ViewOpportunityDetail(Guid id) => Navigation.NavigateTo($"crm/opportunities/{id}");

    private async Task<QueryData<QuotationDto>> OnQueryAsync(QueryPageOptions options)
    {
        try
        {
            var result = await CrmApi.GetQuotationsAsync(new QuotationListQuery
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
            return EmptyQuery<QuotationDto>();
        }
    }

    private Task ShowCreateAsync(IEnumerable<QuotationDto> _)
    {
        _isCreate = true;
        _modalTitle = "Thêm báo giá";
        _model = new QuotationFormModel
        {
            QuotationNo = $"QT-{DateTime.Now:yyyyMMddHHmm}",
            Quantity = 1,
            UnitPrice = 0
        };
        _customerIdText = _customerOptions.FirstOrDefault()?.Value ?? "";
        _contactIdText = null;
        return _editModal!.Show();
    }

    private Task ShowEditAsync(QuotationDto item)
    {
        _isCreate = false;
        _modalTitle = "Sửa báo giá";
        _editing = item;
        _customerIdText = item.CustomerId.ToString();
        _contactIdText = item.ContactId?.ToString();
        var firstLine = item.Lines.FirstOrDefault();
        _model = new QuotationFormModel
        {
            QuotationNo = item.QuotationNo,
            Subject = item.Subject,
            ProductCode = firstLine?.ProductCode ?? "SP001",
            ProductName = firstLine?.ProductName ?? "Sản phẩm",
            Quantity = firstLine?.Quantity ?? 1,
            UnitPrice = firstLine?.UnitPrice ?? 0
        };
        return _editModal!.Show();
    }

    private Task CloseModalAsync() => _editModal!.Close();

    private IReadOnlyList<CreateQuotationLineRequest> BuildLines() =>
    [
        new(1, _model.ProductCode, _model.ProductName, null, _model.Quantity, "EA", _model.UnitPrice, 0, 0, 1)
    ];

    private async Task SaveAsync()
    {
        if (!Guid.TryParse(_customerIdText, out var customerId) || string.IsNullOrWhiteSpace(_model.QuotationNo))
        {
            await ShowErrorAsync(new InvalidOperationException("Vui lòng chọn khách hàng và nhập số báo giá."));
            return;
        }

        try
        {
            Guid? contactId = Guid.TryParse(_contactIdText, out var parsedContactId) ? parsedContactId : null;
            if (_isCreate)
            {
                await CrmApi.CreateQuotationAsync(new CreateQuotationRequest(
                    customerId, _model.QuotationNo.Trim(), null, contactId, _model.Subject, null, BuildLines()));
                await ToastService.Success("Thành công", "Đã tạo báo giá.");
            }
            else if (_editing is not null)
            {
                await CrmApi.UpdateQuotationAsync(_editing.Id, new UpdateQuotationRequest(
                    customerId, _editing.OpportunityId, contactId, _model.Subject,
                    _editing.Description, _editing.DiscountAmount, _editing.DiscountPercent,
                    _editing.ValidUntil, _editing.Notes, _editing.Terms, null, BuildLines()));
                await ToastService.Success("Thành công", "Đã cập nhật báo giá.");
            }

            await _editModal!.Close();
            await ReloadTableAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private async Task ApproveAsync(Guid id)
    {
        try
        {
            await CrmApi.ApproveQuotationAsync(id);
            await ToastService.Success("Thành công", "Đã duyệt báo giá.");
            await ReloadTableAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private void ViewDetailAsync(Guid id) => Navigation.NavigateTo($"crm/quotations/{id}");

    private async Task DeleteAsync(Guid id)
    {
        if (!await ConfirmDeleteAsync())
        {
            return;
        }

        try
        {
            await CrmApi.DeleteQuotationAsync(id);
            await ToastService.Success("Đã xoá", "Báo giá đã được xoá.");
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

    private sealed class QuotationFormModel
    {
        public string QuotationNo { get; set; } = "";
        public string? Subject { get; set; }
        public string ProductCode { get; set; } = "SP001";
        public string ProductName { get; set; } = "Sản phẩm";
        public decimal Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
    }
}
