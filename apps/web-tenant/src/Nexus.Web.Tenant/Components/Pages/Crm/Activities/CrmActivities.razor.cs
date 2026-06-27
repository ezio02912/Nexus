using BootstrapBlazor.Components;

namespace Nexus.Web.Tenant.Components.Pages.Crm.Activities;

public partial class CrmActivities
{
    private Table<ActivityDto>? _table;
    private Modal? _editModal;
    private bool _isCreate;
    private string _modalTitle = "";
    private ActivityDto? _editing;
    private ActivityFormModel _model = new() { ActivityDate = DateTimeOffset.Now };
    private string _relatedTypeText = CrmRelatedEntityType.Customer.ToString();
    private string _activityTypeText = CrmActivityType.Task.ToString();
    private string _statusText = CrmActivityStatus.Planned.ToString();
    private string _relatedEntityIdText = "";
    private List<SelectedItem> _relatedTypeOptions = CrmLabels.RelatedEntityTypeOptions();
    private List<SelectedItem> _activityTypeOptions = CrmLabels.ActivityTypeOptions();
    private List<SelectedItem> _statusOptions = CrmLabels.ActivityStatusOptions();
    private Dictionary<string, (string Text, string BadgeClass)> _statusMeta = CrmLabels.ActivityStatusMeta();

    private async Task<QueryData<ActivityDto>> OnQueryAsync(QueryPageOptions options)
    {
        try
        {
            var result = await CrmApi.GetActivitiesAsync(new ActivityListQuery
            {
                SkipCount = (options.PageIndex - 1) * options.PageItems,
                MaxResultCount = options.PageItems,
                Sorting = options.SortName
            });
            return ToQueryData(result, options);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
            return EmptyQuery<ActivityDto>();
        }
    }

    private Task ShowCreateAsync(IEnumerable<ActivityDto> _)
    {
        _isCreate = true;
        _modalTitle = "Thêm hoạt động";
        _model = new ActivityFormModel { ActivityDate = DateTimeOffset.Now };
        // Reset all selects to their defaults so a previous edit does not leak in.
        _relatedTypeText = CrmRelatedEntityType.Customer.ToString();
        _activityTypeText = CrmActivityType.Task.ToString();
        _statusText = CrmActivityStatus.Planned.ToString();
        _relatedEntityIdText = "";
        return _editModal!.Show();
    }

    private Task ShowEditAsync(ActivityDto item)
    {
        _isCreate = false;
        _modalTitle = "Sửa hoạt động";
        _editing = item;
        _relatedTypeText = item.RelatedEntityType.ToString();
        _activityTypeText = item.ActivityType.ToString();
        _statusText = item.Status.ToString();
        _relatedEntityIdText = item.RelatedEntityId.ToString();
        _model = new ActivityFormModel
        {
            Subject = item.Subject,
            Description = item.Description,
            ActivityDate = item.ActivityDate
        };
        return _editModal!.Show();
    }

    private Task CloseModalAsync() => _editModal!.Close();

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_model.Subject)
            || !Guid.TryParse(_relatedEntityIdText, out var relatedEntityId)
            || !Enum.TryParse<CrmRelatedEntityType>(_relatedTypeText, out var relatedType)
            || !Enum.TryParse<CrmActivityType>(_activityTypeText, out var activityType))
        {
            await ShowErrorAsync(new InvalidOperationException("Vui lòng nhập đầy đủ thông tin bắt buộc."));
            return;
        }

        try
        {
            if (_isCreate)
            {
                await CrmApi.CreateActivityAsync(new CreateActivityRequest(
                    relatedType, relatedEntityId, activityType,
                    _model.Subject.Trim(), _model.ActivityDate, null, null));
                await ToastService.Success("Thành công", "Đã tạo hoạt động.");
            }
            else if (_editing is not null)
            {
                if (!Enum.TryParse<CrmActivityStatus>(_statusText, out var status))
                {
                    status = _editing.Status;
                }

                await CrmApi.UpdateActivityAsync(_editing.Id, new UpdateActivityRequest(
                    activityType, _model.Subject.Trim(), _model.Description,
                    _model.ActivityDate, _editing.DueDate, status, null, null, _editing.DurationMinutes));
                await ToastService.Success("Thành công", "Đã cập nhật hoạt động.");
            }

            await _editModal!.Close();
            await ReloadTableAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private async Task CompleteAsync(Guid id)
    {
        try
        {
            await CrmApi.CompleteActivityAsync(id);
            await ToastService.Success("Thành công", "Đã hoàn thành hoạt động.");
            await ReloadTableAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private async Task DeleteAsync(Guid id)
    {
        if (!await ConfirmDeleteAsync())
        {
            return;
        }

        try
        {
            await CrmApi.DeleteActivityAsync(id);
            await ToastService.Success("Đã xoá", "Hoạt động đã được xoá.");
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

    private sealed class ActivityFormModel
    {
        public string Subject { get; set; } = "";
        public string? Description { get; set; }
        public DateTimeOffset ActivityDate { get; set; }
    }
}
