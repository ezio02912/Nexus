using BootstrapBlazor.Components;

namespace Nexus.Web.Tenant.Components.Pages.Crm;

public partial class CrmOpportunities
{
    private Table<OpportunityDto>? _table;
    private Modal? _editModal;
    private bool _isCreate;
    private string _modalTitle = "";
    private OpportunityDto? _editing;
    private OpportunityFormModel _model = new();
    private string _customerIdText = "";
    private string _stageText = OpportunityStage.Prospecting.ToString();
    private List<SelectedItem> _customerOptions = [];
    private List<SelectedItem> _stageOptions = CrmLabels.OpportunityStageOptions();
    private Dictionary<string, (string Text, string BadgeClass)> _stageMeta = CrmLabels.OpportunityStageMeta();
    private Dictionary<Guid, string> _customerNames = [];

    protected override async Task OnInitializedAsync() => await LoadCustomersAsync();

    private async Task LoadCustomersAsync()
    {
        try
        {
            var result = await CrmApi.GetCustomersAsync(new CustomerListQuery { MaxResultCount = 500 });
            _customerNames = (result?.Items ?? []).ToDictionary(x => x.Id, x => $"{x.Code} - {x.Name}");
            _customerOptions =
            [
                new SelectedItem("", "— Không chọn —"),
                .. _customerNames.Select(x => new SelectedItem(x.Key.ToString(), x.Value))
            ];
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private string CustomerName(Guid? customerId) =>
        customerId.HasValue && _customerNames.TryGetValue(customerId.Value, out var name) ? name : "—";

    private async Task<QueryData<OpportunityDto>> OnQueryAsync(QueryPageOptions options)
    {
        try
        {
            var result = await CrmApi.GetOpportunitiesAsync(new OpportunityListQuery
            {
                Search = options.SearchText,
                SkipCount = (options.PageIndex - 1) * options.PageItems,
                MaxResultCount = options.PageItems,
                Sorting = options.SortName
            });
            return ToQueryData(result, options);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
            return EmptyQuery<OpportunityDto>();
        }
    }

    private Task ShowCreateAsync(IEnumerable<OpportunityDto> _)
    {
        _isCreate = true;
        _modalTitle = "Thêm cơ hội";
        _model = new OpportunityFormModel { Amount = 0 };
        _customerIdText = "";
        return _editModal!.Show();
    }

    private Task ShowEditAsync(OpportunityDto item)
    {
        _isCreate = false;
        _modalTitle = "Sửa cơ hội";
        _editing = item;
        _customerIdText = item.CustomerId?.ToString() ?? "";
        _stageText = item.Stage.ToString();
        _model = new OpportunityFormModel
        {
            Name = item.Name,
            Amount = item.Amount,
            Probability = item.Probability,
            Currency = item.Currency
        };
        return _editModal!.Show();
    }

    private Task CloseModalAsync() => _editModal!.Close();

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_model.Name))
        {
            await ShowErrorAsync(new InvalidOperationException("Vui lòng nhập tên cơ hội."));
            return;
        }

        Guid? customerId = Guid.TryParse(_customerIdText, out var parsed) ? parsed : null;

        try
        {
            if (_isCreate)
            {
                await CrmApi.CreateOpportunityAsync(new CreateOpportunityRequest(
                    customerId, null, _model.Name.Trim(), _model.Amount, null, null));
                await ToastService.Success("Thành công", "Đã tạo cơ hội.");
            }
            else if (_editing is not null)
            {
                await CrmApi.UpdateOpportunityAsync(_editing.Id, new UpdateOpportunityRequest(
                    customerId, _editing.ContactId, _model.Name.Trim(), _model.Amount,
                    _model.Probability, _model.Currency, _editing.ExpectedCloseDate,
                    _editing.Description, _editing.NextStep, _editing.NextStepDate,
                    _editing.Source, _editing.Competitor, null));

                if (Enum.TryParse<OpportunityStage>(_stageText, out var stage) && stage != _editing.Stage)
                {
                    await CrmApi.ChangeOpportunityStageAsync(_editing.Id, new ChangeOpportunityStageRequest(stage, _model.Probability, null, null));
                }

                await ToastService.Success("Thành công", "Đã cập nhật cơ hội.");
            }

            await _editModal!.Close();
            await ReloadTableAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private void ViewDetailAsync(Guid id) => Navigation.NavigateTo($"crm/opportunities/{id}");
    private void GoToBoard() => Navigation.NavigateTo("crm/opportunity-board");

    private async Task DeleteAsync(Guid id)
    {
        if (!await ConfirmDeleteAsync())
        {
            return;
        }

        try
        {
            await CrmApi.DeleteOpportunityAsync(id);
            await ToastService.Success("Đã xoá", "Cơ hội đã được xoá.");
            await ReloadTableAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, "Xoá thất bại");
        }
    }

    private async Task ReloadTableAsync()
    {
        if (_table is not null)
        {
            await _table.QueryAsync();
        }
    }

    private sealed class OpportunityFormModel
    {
        public string Name { get; set; } = "";
        public decimal Amount { get; set; }
        public int Probability { get; set; } = 10;
        public string Currency { get; set; } = "VND";
    }
}
