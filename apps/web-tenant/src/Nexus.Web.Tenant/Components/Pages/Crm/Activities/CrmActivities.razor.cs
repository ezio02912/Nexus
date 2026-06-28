using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;

namespace Nexus.Web.Tenant.Components.Pages.Crm.Activities;

public partial class CrmActivities
{
    [Inject] private TenantPortalApiClient PortalApi { get; set; } = default!;

    private Modal? _editModal;
    private bool _loading = true;
    private bool _isCreate;
    private string _modalTitle = "";
    private ActivityDto? _editing;
    private ActivityFormModel _model = new() { ActivityDate = DateTimeOffset.Now };
    private DateTime _selectedDate = DateTime.Today;
    private string _relatedTypeText = CrmRelatedEntityType.Customer.ToString();
    private string _activityTypeText = CrmActivityType.Task.ToString();
    private string _statusText = CrmActivityStatus.Planned.ToString();
    private string _relatedEntityIdText = "";
    private List<string> _assignedToIdTexts = [];
    private IReadOnlyList<ActivityDto> _activities = [];
    private List<SelectedItem> _relatedTypeOptions = CrmLabels.RelatedEntityTypeOptions();
    private List<SelectedItem> _activityTypeOptions = CrmLabels.ActivityTypeOptions();
    private List<SelectedItem> _statusOptions = CrmLabels.ActivityStatusOptions();
    private Dictionary<string, (string Text, string BadgeClass)> _statusMeta = CrmLabels.ActivityStatusMeta();
    private List<SelectedItem> _userOptions = [];
    private Dictionary<Guid, string> _userNames = [];
    private Dictionary<CrmRelatedEntityType, List<SelectedItem>> _relatedOptions = [];
    private Dictionary<Guid, string> _relatedNames = [];

    private List<ActivityDto> SelectedDateActivities =>
        ActivitiesForDate(_selectedDate)
            .OrderBy(x => x.ActivityDate)
            .ToList();

    private List<SelectedItem> RelatedEntityOptions =>
        Enum.TryParse<CrmRelatedEntityType>(_relatedTypeText, out var type)
            && _relatedOptions.TryGetValue(type, out var items)
            ? items
            : [new("", "Chọn thực thể...")];

    protected override async Task OnInitializedAsync()
    {
        await LoadLookupDataAsync();
        await LoadAsync();
    }

    private async Task LoadLookupDataAsync()
    {
        try
        {
            var customers = (await CrmApi.GetCustomersAsync(new CustomerListQuery { MaxResultCount = 500 }))?.Items ?? [];
            var leads = (await CrmApi.GetLeadsAsync(new LeadListQuery { MaxResultCount = 500 }))?.Items ?? [];
            var opportunities = (await CrmApi.GetOpportunitiesAsync(new OpportunityListQuery { MaxResultCount = 500 }))?.Items ?? [];
            var quotations = (await CrmApi.GetQuotationsAsync(new QuotationListQuery { MaxResultCount = 500 }))?.Items ?? [];
            var contracts = (await CrmApi.GetContractsAsync(new ContractListQuery { MaxResultCount = 500 }))?.Items ?? [];

            _relatedOptions = new Dictionary<CrmRelatedEntityType, List<SelectedItem>>
            {
                [CrmRelatedEntityType.Customer] = [new("", "Chọn khách hàng..."), .. customers.Select(x => RelatedItem(x.Id, $"{x.Code} - {x.Name}"))],
                [CrmRelatedEntityType.Lead] = [new("", "Chọn lead..."), .. leads.Select(x => RelatedItem(x.Id, $"{x.FullName} - {x.CompanyName}"))],
                [CrmRelatedEntityType.Opportunity] = [new("", "Chọn cơ hội..."), .. opportunities.Select(x => RelatedItem(x.Id, x.Name))],
                [CrmRelatedEntityType.Quotation] = [new("", "Chọn báo giá..."), .. quotations.Select(x => RelatedItem(x.Id, $"{x.QuotationNo} - {x.Subject}"))],
                [CrmRelatedEntityType.Contract] = [new("", "Chọn hợp đồng..."), .. contracts.Select(x => RelatedItem(x.Id, $"{x.ContractNo} - {x.Title}"))]
            };

            _relatedNames = _relatedOptions.Values
                .SelectMany(x => x)
                .Where(x => Guid.TryParse(x.Value, out _))
                .GroupBy(x => Guid.Parse(x.Value))
                .ToDictionary(x => x.Key, x => x.First().Text);

            if (Session.TenantId is Guid tenantId)
            {
                var users = (await PortalApi.GetUsersAsync(tenantId))?.Items ?? [];
                _userOptions = users
                    .Where(x => x.IsActive)
                    .Select(x => new SelectedItem(x.Id.ToString(), $"{x.UserName} - {x.Email}"))
                    .ToList();
                _userNames = users.ToDictionary(x => x.Id, x => string.IsNullOrWhiteSpace(x.UserName) ? x.Email : x.UserName);
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    private async Task LoadAsync()
    {
        _loading = true;
        try
        {
            var result = await CrmApi.GetActivitiesAsync(new ActivityListQuery
            {
                SkipCount = 0,
                MaxResultCount = 1000,
                Sorting = "ActivityDate"
            });
            _activities = result?.Items ?? [];
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
            _activities = [];
        }
        finally
        {
            _loading = false;
        }
    }

    private Task OnCalendarDateChanged(DateTime value)
    {
        _selectedDate = value.Date;
        return Task.CompletedTask;
    }

    private Task SelectDateAsync(DateTime value)
    {
        _selectedDate = value.Date;
        return Task.CompletedTask;
    }

    private Task ShowCreateForDateAsync(DateTime date)
    {
        _isCreate = true;
        _modalTitle = "Thêm hoạt động";
        _model = new ActivityFormModel { ActivityDate = new DateTimeOffset(date.Date.AddHours(9)) };
        _relatedTypeText = CrmRelatedEntityType.Customer.ToString();
        _activityTypeText = CrmActivityType.Task.ToString();
        _statusText = CrmActivityStatus.Planned.ToString();
        _relatedEntityIdText = "";
        _assignedToIdTexts = [];
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
        _assignedToIdTexts = item.AssignedToIds.Count > 0
            ? item.AssignedToIds.Select(x => x.ToString()).ToList()
            : item.AssignedToId.HasValue ? [item.AssignedToId.Value.ToString()] : [];
        _model = new ActivityFormModel
        {
            Subject = item.Subject,
            Description = item.Description,
            ActivityDate = item.ActivityDate,
            DueDate = item.DueDate
        };
        return _editModal!.Show();
    }

    private Task CloseModalAsync() => _editModal!.Close();

    private Task OnRelatedTypeSelectedAsync(SelectedItem item)
    {
        _relatedTypeText = item.Value;
        _relatedEntityIdText = "";
        return Task.CompletedTask;
    }

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

        var assignedToIds = _assignedToIdTexts
            .Select(x => Guid.TryParse(x, out var id) ? id : Guid.Empty)
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();
        var firstAssignee = assignedToIds.FirstOrDefault();
        Guid? assignedToId = firstAssignee == Guid.Empty ? null : firstAssignee;

        try
        {
            if (_isCreate)
            {
                await CrmApi.CreateActivityAsync(new CreateActivityRequest(
                    relatedType, relatedEntityId, activityType,
                    _model.Subject.Trim(), _model.ActivityDate, assignedToId, assignedToId, assignedToIds));
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
                    _model.ActivityDate, _model.DueDate, status, assignedToId, assignedToId, assignedToIds, _editing.DurationMinutes));
                await ToastService.Success("Thành công", "Đã cập nhật hoạt động.");
            }

            await _editModal!.Close();
            ResetModalState();
            await LoadAsync();
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
            await LoadAsync();
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
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, "Xoá thất bại");
        }
    }

    private void ResetModalState()
    {
        _editing = null;
        _isCreate = false;
        _model = new ActivityFormModel { ActivityDate = DateTimeOffset.Now };
        _relatedTypeText = CrmRelatedEntityType.Customer.ToString();
        _activityTypeText = CrmActivityType.Task.ToString();
        _statusText = CrmActivityStatus.Planned.ToString();
        _relatedEntityIdText = "";
        _assignedToIdTexts = [];
    }

    private List<ActivityDto> ActivitiesForDate(DateTime value) =>
        _activities
            .Where(x => ActivityStartDate(x) <= value.Date && ActivityEndDate(x) >= value.Date)
            .OrderBy(x => x.ActivityDate)
            .ToList();

    private List<ActivityCalendarSegment> ActivitySegmentsForDate(DateTime value) =>
        ActivitiesForDate(value)
            .Select(x => new ActivityCalendarSegment(
                x,
                ActivityStartDate(x) == value.Date,
                ActivityEndDate(x) == value.Date,
                ActivityStartDate(x) < value.Date && ActivityEndDate(x) > value.Date))
            .OrderBy(x => x.Activity.ActivityDate)
            .ToList();

    private string RelatedEntityName(ActivityDto item) =>
        _relatedNames.TryGetValue(item.RelatedEntityId, out var name) ? name : item.RelatedEntityId.ToString("N")[..8];

    private string AssigneeNames(ActivityDto item)
    {
        var names = item.AssignedToIds
            .Select(x => _userNames.TryGetValue(x, out var name) ? name : x.ToString("N")[..8])
            .ToArray();

        return names.Length == 0 ? "" : string.Join(", ", names);
    }

    private static SelectedItem RelatedItem(Guid id, string text) => new(id.ToString(), text);

    private static DateTime ActivityStartDate(ActivityDto item) => item.ActivityDate.ToLocalTime().Date;

    private static DateTime ActivityEndDate(ActivityDto item)
    {
        var start = ActivityStartDate(item);
        var due = item.DueDate?.ToLocalTime().Date ?? start;
        return due < start ? start : due;
    }

    private static string CalendarEventCss(ActivityCalendarSegment segment)
    {
        var css = segment.Activity.Status switch
        {
            CrmActivityStatus.Completed => "crm-calendar-event is-completed",
            CrmActivityStatus.Cancelled => "crm-calendar-event is-cancelled",
            _ => segment.Activity.ActivityType switch
            {
                CrmActivityType.Meeting => "crm-calendar-event is-meeting",
                CrmActivityType.Call => "crm-calendar-event is-call",
                CrmActivityType.Email => "crm-calendar-event is-email",
                _ => "crm-calendar-event"
            }
        };

        if (segment.IsStart)
        {
            css += " is-range-start";
        }

        if (segment.IsEnd)
        {
            css += " is-range-end";
        }

        if (segment.IsMiddle)
        {
            css += " is-range-middle";
        }

        return css;
    }

    private static string CalendarEventCss(ActivityDto item) => item.Status switch
    {
        CrmActivityStatus.Completed => "crm-calendar-event is-completed",
        CrmActivityStatus.Cancelled => "crm-calendar-event is-cancelled",
        _ => item.ActivityType switch
        {
            CrmActivityType.Meeting => "crm-calendar-event is-meeting",
            CrmActivityType.Call => "crm-calendar-event is-call",
            CrmActivityType.Email => "crm-calendar-event is-email",
            _ => "crm-calendar-event"
        }
    };

    private sealed class ActivityFormModel
    {
        public string Subject { get; set; } = "";
        public string? Description { get; set; }
        public DateTimeOffset ActivityDate { get; set; }
        public DateTimeOffset? DueDate { get; set; }
    }

    private sealed record ActivityCalendarSegment(ActivityDto Activity, bool IsStart, bool IsEnd, bool IsMiddle);
}
