namespace Nexus.Web.Tenant.Services;

public sealed class MasterDataCategories
{
    public const string Industry = "Industry";
    public const string City = "City";
    public const string Source = "Source";
}

public sealed record LookupItemDto(
    Guid Id,
    string Category,
    string Code,
    string Name,
    int SortOrder,
    bool IsActive);

public sealed record PagedLookupItemsDto(long TotalCount, IReadOnlyList<LookupItemDto> Items);

public sealed class LookupListQuery
{
    public string Category { get; set; } = "";
    public string? Search { get; set; }
    public int SkipCount { get; set; }
    public int MaxResultCount { get; set; } = 200;
}
