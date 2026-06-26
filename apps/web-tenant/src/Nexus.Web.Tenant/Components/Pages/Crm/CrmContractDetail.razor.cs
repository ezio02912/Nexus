using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;

namespace Nexus.Web.Tenant.Components.Pages.Crm;

public partial class CrmContractDetail
{
    [Parameter] public Guid Id { get; set; }

    private bool _loading;
    private ContractDto? _item;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        _loading = true;
        try
        {
            _item = await CrmApi.GetContractAsync(Id);
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
}
