namespace Nexus.Web.Tenant.Components.Pages.Crm;

public partial class CrmDashboard
{
    private bool _loading;
    private CrmDashboardDto? _dashboard;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        if (!HasCrmModule)
        {
            return;
        }

        _loading = true;
        try
        {
            _dashboard = await CrmApi.GetDashboardAsync();
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
}
