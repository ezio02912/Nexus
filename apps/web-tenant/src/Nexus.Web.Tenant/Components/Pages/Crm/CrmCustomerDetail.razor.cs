using Microsoft.AspNetCore.Components;

namespace Nexus.Web.Tenant.Components.Pages.Crm;

public partial class CrmCustomerDetail
{
    [Parameter] public Guid Id { get; set; }

    private bool _loading;
    private CustomerDto? _customer;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        _loading = true;
        try
        {
            _customer = await CrmApi.GetCustomerAsync(Id);
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

    private void GoBack() => Navigation.NavigateTo("crm/customers");
}
