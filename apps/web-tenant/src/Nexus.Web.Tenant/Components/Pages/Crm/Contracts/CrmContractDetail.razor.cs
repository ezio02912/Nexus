using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;

namespace Nexus.Web.Tenant.Components.Pages.Crm.Contracts;

public partial class CrmContractDetail
{
    [Parameter] public Guid Id { get; set; }

    private bool _loading;
    private ContractDto? _item;
    private CustomerDto? _customer;
    private OpportunityDto? _opportunity;
    private QuotationDto? _quotation;
    private Modal? _editModal;
    private ContractEditModel _editModel = new();
    private bool IsCompleted => _item?.Status == ContractStatus.Completed;
    private bool CanEditContract => _item is not null && !IsCompleted;
    private bool CanSignContract => _item?.Status is ContractStatus.Draft or ContractStatus.PendingSign;
    private bool CanActivateContract => _item?.Status == ContractStatus.Signed;
    private bool CanCompleteContract => _item?.Status is ContractStatus.Signed or ContractStatus.Active;
    private bool CanTerminateContract => _item?.Status is ContractStatus.Signed or ContractStatus.Active;
    private bool CanEditAttachments => !IsCompleted;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        _loading = true;
        try
        {
            _item = await CrmApi.GetContractAsync(Id);
            _customer = null;
            _opportunity = null;
            _quotation = null;
            if (_item is not null)
            {
                _customer = await CrmApi.GetCustomerAsync(_item.CustomerId);
                if (_item.OpportunityId.HasValue)
                {
                    _opportunity = await CrmApi.GetOpportunityAsync(_item.OpportunityId.Value);
                }

                if (_item.QuotationId.HasValue)
                {
                    _quotation = await CrmApi.GetQuotationAsync(_item.QuotationId.Value);
                }
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

    private async Task SignAsync()
    {
        try
        {
            _item = await CrmApi.SignContractAsync(Id);
            await ToastService.Success("Thành công", "Đã ký hợp đồng.");
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private async Task ActivateAsync()
    {
        try
        {
            _item = await CrmApi.ActivateContractAsync(Id);
            await ToastService.Success("Thành công", "Đã kích hoạt hợp đồng.");
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private async Task TerminateAsync()
    {
        var confirmed = await SwalService.ShowModal(new SwalOption
        {
            Category = SwalCategory.Question,
            Title = "Xác nhận chấm dứt",
            Content = "Bạn có chắc muốn chấm dứt hợp đồng này?"
        });
        if (!confirmed)
        {
            return;
        }

        try
        {
            _item = await CrmApi.TerminateContractAsync(Id, new TerminateContractRequest("Chấm dứt từ giao diện tenant"));
            await ToastService.Success("Thành công", "Đã chấm dứt hợp đồng.");
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private async Task CompleteAsync()
    {
        var confirmed = await SwalService.ShowModal(new SwalOption
        {
            Category = SwalCategory.Question,
            Title = "Xác nhận hoàn thành",
            Content = "Bạn có chắc muốn đánh dấu hợp đồng này là đã hoàn thành?"
        });
        if (!confirmed)
        {
            return;
        }

        try
        {
            _item = await CrmApi.CompleteContractAsync(Id);
            await ToastService.Success("Thành công", "Hợp đồng đã được đánh dấu hoàn thành.");
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private void GoBack() => Navigation.NavigateTo("crm/contracts");
    private void OpenCustomer(Guid id) => Navigation.NavigateTo($"crm/customers/{id}");
    private void OpenOpportunity(Guid id) => Navigation.NavigateTo($"crm/opportunities/{id}");
    private void OpenQuotation(Guid id) => Navigation.NavigateTo($"crm/quotations/{id}");

    private Task ShowEditModalAsync()
    {
        if (_item is null)
        {
            return Task.CompletedTask;
        }

        if (IsCompleted)
        {
            return ShowErrorAsync(new InvalidOperationException("Hợp đồng đã hoàn thành, không thể sửa."));
        }

        _editModel = new ContractEditModel
        {
            Title = _item.Title,
            ContractValue = _item.ContractValue,
            Currency = _item.Currency,
            PaymentTerms = _item.PaymentTerms,
            Notes = _item.Notes,
            // Load every existing line so the user can review/edit them all.
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
        if (_item is null || string.IsNullOrWhiteSpace(_editModel.Title))
        {
            await ShowErrorAsync(new InvalidOperationException("Vui lòng nhập tiêu đề hợp đồng."));
            return;
        }

        if (IsCompleted)
        {
            await ShowErrorAsync(new InvalidOperationException("Hợp đồng đã hoàn thành, không thể sửa."));
            return;
        }

        try
        {
            var lines = _editModel.Lines
                .Where(x => !string.IsNullOrWhiteSpace(x.ProductCode) && !string.IsNullOrWhiteSpace(x.ProductName))
                .Select((x, i) => new CreateContractLineRequest(
                    i + 1,
                    x.ProductCode.Trim(),
                    x.ProductName.Trim(),
                    null,
                    x.Quantity,
                    string.IsNullOrWhiteSpace(x.Unit) ? "Cái" : x.Unit.Trim(),
                    x.UnitPrice,
                    0,
                    0,
                    i + 1))
                .ToArray();

            _item = await CrmApi.UpdateContractAsync(_item.Id, new UpdateContractRequest(
                _item.CustomerId,
                _item.QuotationId,
                _item.OpportunityId,
                _item.ContactId,
                _editModel.Title.Trim(),
                _editModel.ContractValue,
                string.IsNullOrWhiteSpace(_editModel.Currency) ? _item.Currency : _editModel.Currency.Trim(),
                _item.StartDate,
                _item.EndDate,
                _item.RenewalDate,
                _editModel.PaymentTerms,
                _editModel.Notes,
                _item.Terms,
                _item.FileId,
                _item.OwnerId,
                lines));

            await _editModal!.Close();
            await ToastService.Success("Thành công", "Đã cập nhật hợp đồng.");
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
        var description = line?.ProductName ?? _item.Title;
        var quantity = line?.Quantity ?? 1;
        var unitPrice = line?.UnitPrice ?? _item.ContractValue;
        var taxPercent = line?.TaxPercent ?? 0;
        var discountPercent = line?.DiscountPercent ?? 0;
        Navigation.NavigateTo(
            "sales/orders"
            + $"?customerId={_item.CustomerId}"
            + $"&sourceType=contract"
            + $"&sourceId={_item.Id}"
            + $"&sourceNo={Uri.EscapeDataString(_item.ContractNo)}"
            + $"&warehouseCode=MAIN"
            + $"&productCode={Uri.EscapeDataString(productCode)}"
            + $"&description={Uri.EscapeDataString(description)}"
            + $"&quantity={quantity}"
            + $"&unitPrice={unitPrice}"
            + $"&discountPercent={discountPercent}"
            + $"&taxPercent={taxPercent}");
    }

    private sealed class ContractEditModel
    {
        public string Title { get; set; } = "";
        public decimal ContractValue { get; set; }
        public string Currency { get; set; } = "VND";
        public string? PaymentTerms { get; set; }
        public string? Notes { get; set; }
        public List<ProductLineInput> Lines { get; set; } = [];
    }
}
