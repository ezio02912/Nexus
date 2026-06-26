using Microsoft.AspNetCore.Components;

namespace Nexus.Web.Tenant.Components.Pages.Crm;

public partial class CrmQuotationDetail
{
    [Parameter] public Guid Id { get; set; }

    private bool _loading;
    private QuotationDto? _item;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        _loading = true;
        try
        {
            _item = await CrmApi.GetQuotationAsync(Id);
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
}
