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
        Navigation.NavigateTo(
            "sales/orders"
            + $"?customerId={_item.CustomerId}"
            + $"&sourceType=contract"
            + $"&sourceId={_item.Id}"
            + $"&sourceNo={Uri.EscapeDataString(_item.ContractNo)}"
            + $"&productCode={Uri.EscapeDataString(productCode)}"
            + $"&description={Uri.EscapeDataString(description)}"
            + $"&quantity={quantity}"
            + $"&unitPrice={unitPrice}");
    }
}
