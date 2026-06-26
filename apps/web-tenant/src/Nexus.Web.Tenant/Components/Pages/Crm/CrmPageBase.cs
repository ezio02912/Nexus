using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Nexus.ApiContracts.Permissions;
using Nexus.Web.Tenant.Services;

namespace Nexus.Web.Tenant.Components.Pages.Crm;

public partial class CrmPageBase : ComponentBase
{
    [Inject] protected CrmApiClient CrmApi { get; set; } = default!;
    [Inject] protected SwalService SwalService { get; set; } = default!;
    [Inject] protected ToastService ToastService { get; set; } = default!;
    [Inject] protected TenantSessionService Session { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;
    [Inject] protected DialogService DialogService { get; set; } = default!;

    protected static readonly int[] PageItems = [10, 20, 50];
    protected const string AccessDeniedMessage = "Bạn không có quyền truy cập chức năng này.";

    protected bool HasCrmModule => Session.HasModule("CRM");

    protected bool Can(string permission) => Session.IsGranted(permission);

    protected Task ShowErrorAsync(Exception ex, string title = "Đã xảy ra lỗi") =>
        SwalService.Show(new SwalOption
        {
            Category = SwalCategory.Error,
            Title = title,
            Content = ex.Message,
            CloseButtonText = "Đóng"
        });

    protected Task<bool> ConfirmDeleteAsync(string content = "Bạn có chắc muốn xoá bản ghi này?") =>
        SwalService.ShowModal(new SwalOption
        {
            Category = SwalCategory.Question,
            Title = "Xác nhận xoá",
            Content = content,
            ConfirmButtonText = "Xoá",
            CancelButtonText = "Huỷ",
            CloseButtonText = "Đóng"
        });

    protected static QueryData<T> EmptyQuery<T>() => new() { Items = [], TotalCount = 0 };

    protected static QueryData<T> ToQueryData<T>(PagedResultDto<T>? result, QueryPageOptions options)
    {
        return new QueryData<T>
        {
            Items = result?.Items ?? [],
            TotalCount = (int)(result?.TotalCount ?? 0),
            IsSorted = !string.IsNullOrEmpty(options.SortName),
            IsFiltered = !string.IsNullOrEmpty(options.SearchText),
            IsSearch = !string.IsNullOrEmpty(options.SearchText)
        };
    }
}
