using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;

namespace Nexus.Web.Tenant.Components.Pages.Crm;

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

        var line = _item.Lines.FirstOrDefault();
        _editModel = new ContractEditModel
        {
            Title = _item.Title,
            ContractValue = _item.ContractValue,
            Currency = _item.Currency,
            PaymentTerms = _item.PaymentTerms,
            Notes = _item.Notes,
            ProductCode = line?.ProductCode ?? "SP001",
            ProductName = line?.ProductName ?? _item.Title,
            Quantity = line?.Quantity ?? 1,
            UnitPrice = line?.UnitPrice ?? _item.ContractValue
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

        try
        {
            CreateContractLineRequest[] lines = string.IsNullOrWhiteSpace(_editModel.ProductCode) || string.IsNullOrWhiteSpace(_editModel.ProductName)
                ? []
                :
                [
                    new CreateContractLineRequest(
                        1,
                        _editModel.ProductCode.Trim(),
                        _editModel.ProductName.Trim(),
                        null,
                        _editModel.Quantity,
                        "EA",
                        _editModel.UnitPrice,
                        0,
                        0,
                        1)
                ];

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
        public string ProductCode { get; set; } = "SP001";
        public string ProductName { get; set; } = "";
        public decimal Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
    }
}
