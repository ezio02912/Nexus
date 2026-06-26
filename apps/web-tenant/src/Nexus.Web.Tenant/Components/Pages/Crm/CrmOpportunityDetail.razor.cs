using Microsoft.AspNetCore.Components;

namespace Nexus.Web.Tenant.Components.Pages.Crm;

public partial class CrmOpportunityDetail
{
    [Parameter] public Guid Id { get; set; }

    private bool _loading;
    private OpportunityDto? _item;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        _loading = true;
        try
        {
            _item = await CrmApi.GetOpportunityAsync(Id);
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

    private void GoBack() => Navigation.NavigateTo("crm/opportunities");
}
