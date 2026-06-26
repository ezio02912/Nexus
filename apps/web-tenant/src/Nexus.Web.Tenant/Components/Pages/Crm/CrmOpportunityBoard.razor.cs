namespace Nexus.Web.Tenant.Components.Pages.Crm;

public partial class CrmOpportunityBoard
{
    private bool _loading;
    private List<OpportunityDto> _items = [];
    private readonly OpportunityStage[] _stages = Enum.GetValues<OpportunityStage>();

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
            var result = await CrmApi.GetOpportunitiesAsync(new OpportunityListQuery { MaxResultCount = 500 });
            _items = result?.Items.ToList() ?? [];
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

    private IEnumerable<OpportunityDto> ItemsForStage(OpportunityStage stage) =>
        _items.Where(x => x.Stage == stage);

    private int ColumnCount(OpportunityStage stage) => ItemsForStage(stage).Count();

    private async Task ChangeStageAsync(OpportunityDto item, OpportunityStage targetStage)
    {
        try
        {
            await CrmApi.ChangeOpportunityStageAsync(item.Id, new ChangeOpportunityStageRequest(targetStage, item.Probability, null, null));
            await ToastService.Success("Thành công", $"Đã chuyển sang {CrmLabels.FormatOpportunityStage(targetStage)}.");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private void ViewDetailAsync(Guid id) => Navigation.NavigateTo($"crm/opportunities/{id}");
    private void GoToList() => Navigation.NavigateTo("crm/opportunities");
}
