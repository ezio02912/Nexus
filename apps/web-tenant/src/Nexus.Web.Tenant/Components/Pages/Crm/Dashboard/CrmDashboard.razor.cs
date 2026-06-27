using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Nexus.Web.Tenant.Components.Pages.Crm.Dashboard;

public partial class CrmDashboard : IAsyncDisposable
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private const string StageCountChartId = "crm-stage-count-chart";
    private const string StageValueChartId = "crm-stage-value-chart";
    private const string ModulePath = "./Components/Pages/Crm/Dashboard/CrmDashboard.razor.js";

    private bool _loading;
    private bool _chartsDirty;
    private CrmDashboardDto? _dashboard;
    private IJSObjectReference? _module;

    // Distinct, saturated color per pipeline stage so bar/doughnut series are easy
    // to tell apart and carry meaning (won = green, lost = red, etc.).
    private static string StageColor(OpportunityStage stage) => stage switch
    {
        OpportunityStage.Prospecting => "#0EA5E9",   // Tiếp cận - sky blue
        OpportunityStage.Qualification => "#14B8A6", // Đánh giá - teal
        OpportunityStage.Proposal => "#6366F1",      // Đề xuất - indigo
        OpportunityStage.Negotiation => "#F59E0B",   // Đàm phán - amber
        OpportunityStage.ClosedWon => "#16A34A",     // Thắng - green
        OpportunityStage.ClosedLost => "#DC2626",    // Thua - red
        _ => "#64748B"
    };

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
            // Defer chart rendering until the host <div>s exist in the DOM.
            _chartsDirty = _dashboard?.StageFunnelItems.Any() == true;
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_chartsDirty)
        {
            return;
        }

        _chartsDirty = false;
        await RenderChartsAsync();
    }

    private async Task RenderChartsAsync()
    {
        var items = _dashboard?.StageFunnelItems;
        if (items is null || items.Count == 0)
        {
            return;
        }

        try
        {
            _module ??= await JS.InvokeAsync<IJSObjectReference>("import", ModulePath);

            var countSeries = items.Select(x => new
            {
                Label = CrmLabels.FormatOpportunityStage(x.Stage),
                Count = x.Count,
                Color = StageColor(x.Stage)
            });
            var valueSeries = items.Select(x => new
            {
                Label = CrmLabels.FormatOpportunityStage(x.Stage),
                Amount = x.TotalAmount,
                Color = StageColor(x.Stage)
            });

            await _module.InvokeVoidAsync("renderStageCountBar", StageCountChartId, countSeries);
            await _module.InvokeVoidAsync("renderStageValueDonut", StageValueChartId, valueSeries);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is null)
        {
            return;
        }

        try
        {
            await _module.InvokeVoidAsync("disposeAll");
            await _module.DisposeAsync();
        }
        catch
        {
            // Ignore JS interop failures during teardown (e.g. circuit already gone).
        }
    }
}
