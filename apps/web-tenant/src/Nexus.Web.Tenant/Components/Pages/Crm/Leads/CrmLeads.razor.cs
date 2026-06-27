using BootstrapBlazor.Components;

namespace Nexus.Web.Tenant.Components.Pages.Crm.Leads;

public partial class CrmLeads
{
    private Table<LeadDto>? _table;
    private Modal? _editModal;
    private bool _isCreate;
    private string _modalTitle = "";
    private LeadDto? _editing;
    private LeadFormModel _model = new();
    private string _statusText = LeadStatus.New.ToString();
    private List<SelectedItem> _statusOptions = CrmLabels.LeadStatusOptions();
    private Dictionary<string, (string Text, string BadgeClass)> _statusMeta = CrmLabels.LeadStatusMeta();

    private async Task<QueryData<LeadDto>> OnQueryAsync(QueryPageOptions options)
    {
        try
        {
            var result = await CrmApi.GetLeadsAsync(new LeadListQuery
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
            return EmptyQuery<LeadDto>();
        }
    }

    private Task ShowCreateAsync(IEnumerable<LeadDto> _)
    {
        _isCreate = true;
        _modalTitle = "Thêm lead";
        _model = new LeadFormModel();
        return _editModal!.Show();
    }

    private Task ShowEditAsync(LeadDto lead)
    {
        _isCreate = false;
        _modalTitle = "Sửa lead";
        _editing = lead;
        _statusText = lead.Status.ToString();
        _model = new LeadFormModel
        {
            FullName = lead.FullName,
            CompanyName = lead.CompanyName,
            Title = lead.Title,
            Email = lead.Email,
            Phone = lead.Phone,
            Source = lead.Source,
            LeadScore = lead.LeadScore
        };
        return _editModal!.Show();
    }

    private Task CloseModalAsync() => _editModal!.Close();

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_model.FullName))
        {
            await ShowErrorAsync(new InvalidOperationException("Vui lòng nhập họ tên lead."));
            return;
        }

        try
        {
            if (_isCreate)
            {
                await CrmApi.CreateLeadAsync(new CreateLeadRequest(
                    _model.FullName.Trim(), _model.CompanyName, _model.Title,
                    _model.Email, _model.Phone, _model.Source, null));
                await ToastService.Success("Thành công", "Đã tạo lead.");
            }
            else if (_editing is not null)
            {
                if (!Enum.TryParse<LeadStatus>(_statusText, out var status))
                {
                    status = _editing.Status;
                }

                await CrmApi.UpdateLeadAsync(_editing.Id, new UpdateLeadRequest(
                    _model.FullName.Trim(), _model.CompanyName, _model.Title,
                    _model.Email, _model.Phone, _model.Source, _model.LeadScore,
                    _editing.Rating, status, null, _editing.Description,
                    _editing.Address, _editing.City, _editing.Country, _editing.LostReason));
                await ToastService.Success("Thành công", "Đã cập nhật lead.");
            }

            await _editModal!.Close();
            await ReloadTableAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private void ViewDetailAsync(Guid id) => Navigation.NavigateTo($"crm/leads/{id}");

    private void ViewCustomerDetail(Guid id) => Navigation.NavigateTo($"crm/customers/{id}");

    private async Task DeleteAsync(Guid id)
    {
        if (!await ConfirmDeleteAsync())
        {
            return;
        }

        try
        {
            await CrmApi.DeleteLeadAsync(id);
            await ToastService.Success("Đã xoá", "Lead đã được xoá.");
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

    private sealed class LeadFormModel
    {
        public string FullName { get; set; } = "";
        public string? CompanyName { get; set; }
        public string? Title { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Source { get; set; }
        public int LeadScore { get; set; }
    }
}
