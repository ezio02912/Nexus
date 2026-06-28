using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;

namespace Nexus.Web.Tenant.Components.Pages.Crm.Quotations;

public partial class CrmQuotationDetail
{
    [Parameter] public Guid Id { get; set; }

    private bool _loading;
    private QuotationDto? _item;
    private CustomerDto? _customer;
    private OpportunityDto? _opportunity;
    private IReadOnlyList<ContractDto> _contracts = [];
    private Modal? _editModal;
    private QuotationEditModel _editModel = new();
    private Modal? _contractModal;
    private QuickContractModel _contractModel = new();

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        _loading = true;
        try
        {
            _item = await CrmApi.GetQuotationAsync(Id);
            _customer = null;
            _opportunity = null;
            _contracts = [];
            if (_item is not null)
            {
                _customer = await CrmApi.GetCustomerAsync(_item.CustomerId);
                if (_item.OpportunityId.HasValue)
                {
                    _opportunity = await CrmApi.GetOpportunityAsync(_item.OpportunityId.Value);
                }

                _contracts = (await CrmApi.GetContractsAsync(new ContractListQuery
                {
                    CustomerId = _item.CustomerId,
                    MaxResultCount = 50
                }))?.Items.Where(x => x.QuotationId == Id).ToArray() ?? [];
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

    private async Task ApproveAsync()
    {
        try
        {
            _item = await CrmApi.ApproveQuotationAsync(Id);
            await ToastService.Success("Thành công", "Đã duyệt báo giá.");
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private async Task RejectAsync()
    {
        try
        {
            _item = await CrmApi.RejectQuotationAsync(Id, new RejectQuotationRequest("Từ chối từ giao diện tenant"));
            await ToastService.Success("Thành công", "Đã từ chối báo giá.");
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private async Task SendAsync()
    {
        try
        {
            _item = await CrmApi.SendQuotationAsync(Id);
            await ToastService.Success("Thành công", "Đã gửi báo giá.");
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private void GoBack() => Navigation.NavigateTo("crm/quotations");
    private void OpenCustomer(Guid id) => Navigation.NavigateTo($"crm/customers/{id}");
    private void OpenOpportunity(Guid id) => Navigation.NavigateTo($"crm/opportunities/{id}");
    private void OpenContract(Guid id) => Navigation.NavigateTo($"crm/contracts/{id}");

    private Task ShowEditModalAsync()
    {
        if (_item is null)
        {
            return Task.CompletedTask;
        }

        _editModel = new QuotationEditModel
        {
            Subject = _item.Subject,
            DiscountAmount = _item.DiscountAmount,
            DiscountPercent = _item.DiscountPercent,
            Notes = _item.Notes,
            // Load all existing lines so the user can review/edit them.
            Lines = _item.Lines
                .OrderBy(x => x.LineNo)
                .Select(x => new ProductLineInput
                {
                    ProductCode = x.ProductCode,
                    ProductName = x.ProductName,
                    Unit = string.IsNullOrWhiteSpace(x.Unit) ? "Cái" : x.Unit,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice
                })
                .ToList()
        };
        return _editModal!.Show();
    }

    private Task CloseEditModalAsync() => _editModal!.Close();

    private async Task SaveEditAsync()
    {
        if (_item is null)
        {
            return;
        }

        var lines = _editModel.Lines
            .Where(x => !string.IsNullOrWhiteSpace(x.ProductCode) && !string.IsNullOrWhiteSpace(x.ProductName))
            .Select((x, i) => new CreateQuotationLineRequest(
                i + 1, x.ProductCode.Trim(), x.ProductName.Trim(), null, x.Quantity,
                string.IsNullOrWhiteSpace(x.Unit) ? "Cái" : x.Unit.Trim(), x.UnitPrice, 0, 0, i + 1))
            .ToArray();

        if (lines.Length == 0)
        {
            await ShowErrorAsync(new InvalidOperationException("Vui lòng thêm ít nhất một dòng sản phẩm."));
            return;
        }

        try
        {
            _item = await CrmApi.UpdateQuotationAsync(_item.Id, new UpdateQuotationRequest(
                _item.CustomerId,
                _item.OpportunityId,
                _item.ContactId,
                _editModel.Subject,
                _item.Description,
                _editModel.DiscountAmount,
                _editModel.DiscountPercent,
                _item.ValidUntil,
                _editModel.Notes,
                _item.Terms,
                _item.OwnerId,
                lines));

            await _editModal!.Close();
            await ToastService.Success("Thành công", "Đã cập nhật báo giá.");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private void GoToSalesOrder()
    {
        if (_item is null)
        {
            return;
        }

        var line = _item.Lines.FirstOrDefault();
        var productCode = line?.ProductCode ?? "SKU-001";
        var description = line?.ProductName ?? _item.Subject ?? _item.QuotationNo;
        var quantity = line?.Quantity ?? 1;
        var unitPrice = line?.UnitPrice ?? _item.TotalAmount;
        var taxPercent = line?.TaxPercent ?? 0;
        var discountPercent = line?.DiscountPercent ?? 0;
        Navigation.NavigateTo(
            "sales/orders"
            + $"?customerId={_item.CustomerId}"
            + $"&sourceType=quotation"
            + $"&sourceId={_item.Id}"
            + $"&sourceNo={Uri.EscapeDataString(_item.QuotationNo)}"
            + $"&warehouseCode=MAIN"
            + $"&productCode={Uri.EscapeDataString(productCode)}"
            + $"&description={Uri.EscapeDataString(description)}"
            + $"&quantity={quantity}"
            + $"&unitPrice={unitPrice}"
            + $"&discountPercent={discountPercent}"
            + $"&taxPercent={taxPercent}");
    }

    private Task ShowCreateContractAsync()
    {
        if (_item is null)
        {
            return Task.CompletedTask;
        }

        _contractModel = new QuickContractModel
        {
            Title = string.IsNullOrWhiteSpace(_item.Subject) ? $"Hợp đồng - {_item.QuotationNo}" : _item.Subject,
            ContractValue = _item.TotalAmount,
            StartDate = DateTime.Today
        };
        return _contractModal!.Show();
    }

    private Task CloseContractModalAsync() => _contractModal!.Close();

    private async Task CreateContractAsync()
    {
        if (_item is null || string.IsNullOrWhiteSpace(_contractModel.Title))
        {
            await ShowErrorAsync(new InvalidOperationException("Vui lòng nhập tiêu đề hợp đồng."));
            return;
        }

        try
        {
            var lines = _item.Lines.Select(line => new CreateContractLineRequest(
                line.LineNo,
                line.ProductCode,
                line.ProductName,
                line.Description,
                line.Quantity,
                line.Unit,
                line.UnitPrice,
                line.DiscountPercent,
                line.TaxPercent,
                line.SortOrder)).ToArray();

            var contract = await CrmApi.CreateContractAsync(new CreateContractRequest(
                _item.CustomerId,
                "AUTO",
                _contractModel.Title.Trim(),
                _item.Id,
                _item.OpportunityId,
                _item.ContactId,
                _contractModel.ContractValue,
                "VND",
                ToDateOnly(_contractModel.StartDate),
                ToDateOnly(_contractModel.EndDate),
                null,
                _contractModel.PaymentTerms,
                null,
                _contractModel.Terms,
                null,
                null,
                lines));

            await _contractModal!.Close();
            await ToastService.Success("Thành công", "Đã tạo hợp đồng từ báo giá.");
            if (contract is not null)
            {
                await MarkOpportunityWonAsync(_item.OpportunityId);
                Navigation.NavigateTo($"crm/contracts/{contract.Id}");
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

    private sealed class QuickContractModel
    {
        public string Title { get; set; } = "";
        public decimal ContractValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? PaymentTerms { get; set; }
        public string? Terms { get; set; }
    }

    private static DateOnly? ToDateOnly(DateTime? value) =>
        value.HasValue ? DateOnly.FromDateTime(value.Value) : null;

    private sealed class QuotationEditModel
    {
        public string? Subject { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercent { get; set; }
        public string? Notes { get; set; }
        public List<ProductLineInput> Lines { get; set; } = [];
    }
}
