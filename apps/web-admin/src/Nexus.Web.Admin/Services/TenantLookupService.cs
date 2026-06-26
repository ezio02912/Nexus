using BootstrapBlazor.Components;

namespace Nexus.Web.Admin.Services;

/// <summary>
/// Loads tenants once per circuit and exposes them as <see cref="SelectedItem"/> options
/// (value = tenant Id, text = "CODE — Name") plus a resolver to render the code/name for a
/// tenant Id in tables. This keeps every dropdown showing the tenant code instead of a raw GUID.
/// </summary>
public sealed class TenantLookupService
{
    private readonly CoreApiClient _api;
    private List<TenantDto>? _cache;

    public TenantLookupService(CoreApiClient api) => _api = api;

    /// <summary>Returns the tenant list, loading (and caching) it on first use.</summary>
    public async Task<IReadOnlyList<TenantDto>> GetTenantsAsync(bool forceRefresh = false)
    {
        if (_cache is null || forceRefresh)
        {
            _cache = (await _api.GetTenantsAsync())?.Items.ToList() ?? [];
        }

        return _cache;
    }

    /// <summary>Tenant options for a <c>Select</c>: value is the Id, text is "CODE — Name".</summary>
    public async Task<List<SelectedItem>> GetOptionsAsync(bool includeEmpty = false, bool forceRefresh = false)
    {
        var tenants = await GetTenantsAsync(forceRefresh);
        var items = new List<SelectedItem>();
        if (includeEmpty)
        {
            items.Add(new SelectedItem("", "— Không chọn —"));
        }

        items.AddRange(tenants.Select(t => new SelectedItem(t.Id.ToString(), $"{t.Code} — {t.Name}")));
        return items;
    }

    /// <summary>Resolves a tenant Id to "CODE — Name" for display in a table cell.</summary>
    public string Describe(Guid? id)
    {
        if (id is null || id == Guid.Empty)
        {
            return "—";
        }

        var match = _cache?.FirstOrDefault(t => t.Id == id.Value);
        return match is null ? id.Value.ToString() : $"{match.Code} — {match.Name}";
    }
}
