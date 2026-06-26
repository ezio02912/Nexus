using BootstrapBlazor.Components;

namespace Nexus.Web.Tenant.Services;

public static class CrmLabels
{
    public static string FormatCustomerType(CustomerType value) => value switch
    {
        CustomerType.Company => "Doanh nghiệp",
        CustomerType.Individual => "Cá nhân",
        _ => value.ToString()
    };

    public static string FormatCustomerStatus(CustomerStatus value) => value switch
    {
        CustomerStatus.Active => "Đang hoạt động",
        CustomerStatus.Inactive => "Ngừng hoạt động",
        CustomerStatus.Prospect => "Tiềm năng",
        _ => value.ToString()
    };

    public static string FormatLeadStatus(LeadStatus value) => value switch
    {
        LeadStatus.New => "Mới",
        LeadStatus.Contacted => "Đã liên hệ",
        LeadStatus.Qualified => "Đủ điều kiện",
        LeadStatus.Unqualified => "Không đủ điều kiện",
        LeadStatus.Converted => "Đã chuyển đổi",
        LeadStatus.Lost => "Thất bại",
        _ => value.ToString()
    };

    public static string FormatLeadRating(LeadRating value) => value switch
    {
        LeadRating.Hot => "Nóng",
        LeadRating.Warm => "Ấm",
        LeadRating.Cold => "Lạnh",
        _ => value.ToString()
    };

    public static string FormatOpportunityStage(OpportunityStage value) => value switch
    {
        OpportunityStage.Prospecting => "Tiếp cận",
        OpportunityStage.Qualification => "Đánh giá",
        OpportunityStage.Proposal => "Đề xuất",
        OpportunityStage.Negotiation => "Đàm phán",
        OpportunityStage.ClosedWon => "Thắng",
        OpportunityStage.ClosedLost => "Thua",
        _ => value.ToString()
    };

    public static string FormatQuotationStatus(QuotationStatus value) => value switch
    {
        QuotationStatus.Draft => "Nháp",
        QuotationStatus.Sent => "Đã gửi",
        QuotationStatus.Approved => "Đã duyệt",
        QuotationStatus.Rejected => "Từ chối",
        QuotationStatus.Expired => "Hết hạn",
        QuotationStatus.Cancelled => "Đã huỷ",
        _ => value.ToString()
    };

    public static string FormatContractStatus(ContractStatus value) => value switch
    {
        ContractStatus.Draft => "Nháp",
        ContractStatus.PendingSign => "Chờ ký",
        ContractStatus.Signed => "Đã ký",
        ContractStatus.Active => "Đang hiệu lực",
        ContractStatus.Expired => "Hết hạn",
        ContractStatus.Terminated => "Chấm dứt",
        ContractStatus.Cancelled => "Đã huỷ",
        _ => value.ToString()
    };

    public static string FormatActivityType(CrmActivityType value) => value switch
    {
        CrmActivityType.Call => "Cuộc gọi",
        CrmActivityType.Email => "Email",
        CrmActivityType.Meeting => "Cuộc họp",
        CrmActivityType.Task => "Công việc",
        CrmActivityType.Note => "Ghi chú",
        _ => value.ToString()
    };

    public static string FormatActivityStatus(CrmActivityStatus value) => value switch
    {
        CrmActivityStatus.Planned => "Đã lên lịch",
        CrmActivityStatus.Completed => "Hoàn thành",
        CrmActivityStatus.Cancelled => "Đã huỷ",
        _ => value.ToString()
    };

    public static string FormatRelatedEntityType(CrmRelatedEntityType value) => value switch
    {
        CrmRelatedEntityType.Customer => "Khách hàng",
        CrmRelatedEntityType.Lead => "Lead",
        CrmRelatedEntityType.Opportunity => "Cơ hội",
        CrmRelatedEntityType.Quotation => "Báo giá",
        CrmRelatedEntityType.Contract => "Hợp đồng",
        _ => value.ToString()
    };

    // First declared customer type, used as the default selection on the create form.
    public static string DefaultCustomerType() => Enum.GetValues<CustomerType>().First().ToString();

    public static List<SelectedItem> CustomerTypeOptions(bool includePlaceholder = false)
    {
        var items = Enum.GetValues<CustomerType>()
            .Select(x => new SelectedItem(x.ToString(), FormatCustomerType(x)))
            .ToList();

        if (includePlaceholder)
        {
            items.Insert(0, new SelectedItem("", "Chọn loại khách hàng..."));
        }

        return items;
    }

    public static List<SelectedItem> CustomerStatusOptions() =>
        Enum.GetValues<CustomerStatus>().Select(x => new SelectedItem(x.ToString(), FormatCustomerStatus(x))).ToList();

    public static List<SelectedItem> LeadStatusOptions() =>
        Enum.GetValues<LeadStatus>().Select(x => new SelectedItem(x.ToString(), FormatLeadStatus(x))).ToList();

    public static List<SelectedItem> LeadRatingOptions() =>
        Enum.GetValues<LeadRating>().Select(x => new SelectedItem(x.ToString(), FormatLeadRating(x))).ToList();

    public static List<SelectedItem> OpportunityStageOptions() =>
        Enum.GetValues<OpportunityStage>().Select(x => new SelectedItem(x.ToString(), FormatOpportunityStage(x))).ToList();

    public static List<SelectedItem> QuotationStatusOptions() =>
        Enum.GetValues<QuotationStatus>().Select(x => new SelectedItem(x.ToString(), FormatQuotationStatus(x))).ToList();

    public static List<SelectedItem> ContractStatusOptions() =>
        Enum.GetValues<ContractStatus>().Select(x => new SelectedItem(x.ToString(), FormatContractStatus(x))).ToList();

    public static List<SelectedItem> ActivityTypeOptions() =>
        Enum.GetValues<CrmActivityType>().Select(x => new SelectedItem(x.ToString(), FormatActivityType(x))).ToList();

    public static List<SelectedItem> ActivityStatusOptions() =>
        Enum.GetValues<CrmActivityStatus>().Select(x => new SelectedItem(x.ToString(), FormatActivityStatus(x))).ToList();

    public static List<SelectedItem> RelatedEntityTypeOptions() =>
        Enum.GetValues<CrmRelatedEntityType>().Select(x => new SelectedItem(x.ToString(), FormatRelatedEntityType(x))).ToList();

    public static string StatusBadgeClass(CustomerStatus status) => status switch
    {
        CustomerStatus.Active => "app-status-badge app-status-active",
        CustomerStatus.Prospect => "app-status-badge app-status-pending",
        _ => "app-status-badge app-status-muted"
    };

    public static string StatusBadgeClass(LeadStatus status) => status switch
    {
        LeadStatus.Converted => "app-status-badge app-status-active",
        LeadStatus.Lost or LeadStatus.Unqualified => "app-status-badge app-status-danger",
        LeadStatus.Qualified => "app-status-badge app-status-active",
        _ => "app-status-badge app-status-pending"
    };

    public static string StatusBadgeClass(QuotationStatus status) => status switch
    {
        QuotationStatus.Approved => "app-status-badge app-status-active",
        QuotationStatus.Rejected or QuotationStatus.Cancelled => "app-status-badge app-status-danger",
        QuotationStatus.Sent => "app-status-badge app-status-pending",
        _ => "app-status-badge app-status-muted"
    };

    public static string StatusBadgeClass(ContractStatus status) => status switch
    {
        ContractStatus.Active or ContractStatus.Signed => "app-status-badge app-status-active",
        ContractStatus.Terminated or ContractStatus.Cancelled => "app-status-badge app-status-danger",
        ContractStatus.PendingSign => "app-status-badge app-status-pending",
        _ => "app-status-badge app-status-muted"
    };

    public static Dictionary<string, (string Text, string BadgeClass)> CustomerStatusMeta() =>
        Enum.GetValues<CustomerStatus>().ToDictionary(
            x => x.ToString(),
            x => (FormatCustomerStatus(x), StatusBadgeClass(x)));

    public static Dictionary<string, (string Text, string BadgeClass)> LeadStatusMeta() =>
        Enum.GetValues<LeadStatus>().ToDictionary(
            x => x.ToString(),
            x => (FormatLeadStatus(x), StatusBadgeClass(x)));

    public static Dictionary<string, (string Text, string BadgeClass)> QuotationStatusMeta() =>
        Enum.GetValues<QuotationStatus>().ToDictionary(
            x => x.ToString(),
            x => (FormatQuotationStatus(x), StatusBadgeClass(x)));

    public static Dictionary<string, (string Text, string BadgeClass)> ContractStatusMeta() =>
        Enum.GetValues<ContractStatus>().ToDictionary(
            x => x.ToString(),
            x => (FormatContractStatus(x), StatusBadgeClass(x)));

    public static string StatusBadgeClass(OpportunityStage stage) => stage switch
    {
        OpportunityStage.ClosedWon => "app-status-badge app-status-active",
        OpportunityStage.ClosedLost => "app-status-badge app-status-danger",
        OpportunityStage.Negotiation or OpportunityStage.Proposal => "app-status-badge app-status-pending",
        _ => "app-status-badge app-status-muted"
    };

    public static Dictionary<string, (string Text, string BadgeClass)> OpportunityStageMeta() =>
        Enum.GetValues<OpportunityStage>().ToDictionary(
            x => x.ToString(),
            x => (FormatOpportunityStage(x), StatusBadgeClass(x)));

    public static string StatusBadgeClass(CrmActivityStatus status) => status switch
    {
        CrmActivityStatus.Completed => "app-status-badge app-status-active",
        CrmActivityStatus.Cancelled => "app-status-badge app-status-danger",
        _ => "app-status-badge app-status-pending"
    };

    public static Dictionary<string, (string Text, string BadgeClass)> ActivityStatusMeta() =>
        Enum.GetValues<CrmActivityStatus>().ToDictionary(
            x => x.ToString(),
            x => (FormatActivityStatus(x), StatusBadgeClass(x)));
}
