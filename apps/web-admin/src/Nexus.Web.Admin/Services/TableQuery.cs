using BootstrapBlazor.Components;

namespace Nexus.Web.Admin.Services;

/// <summary>
/// Helper that applies BootstrapBlazor table query options (search, filter, sort, paging)
/// to an in-memory collection so pages can serve full-featured tables from cached API data.
/// </summary>
public static class TableQuery
{
    public static QueryData<T> Apply<T>(IEnumerable<T> source, QueryPageOptions options)
    {
        var items = source;

        // Toolbar fuzzy search across searchable columns
        if (options.Searches.Count > 0)
        {
            items = items.Where(options.Searches.GetFilterFunc<T>(FilterLogic.Or));
        }

        // Advanced / custom search conditions
        if (options.CustomerSearches.Count > 0)
        {
            items = items.Where(options.CustomerSearches.GetFilterFunc<T>());
        }

        // Per-column filters
        if (options.Filters.Count > 0)
        {
            items = items.Where(options.Filters.GetFilterFunc<T>());
        }

        // Sorting (single column header sort takes precedence over default multi-sort)
        if (options.SortOrder != SortOrder.Unset && !string.IsNullOrEmpty(options.SortName))
        {
            var invoker = Utility.GetSortFunc<T>();
            items = invoker(items, options.SortName, options.SortOrder);
        }
        else if (options.SortList.Count > 0)
        {
            var invoker = Utility.GetSortListFunc<T>();
            items = invoker(items, options.SortList);
        }

        var list = items.ToList();
        var total = list.Count;

        // Paging
        if (options.IsPage)
        {
            list = list.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();
        }

        return new QueryData<T>
        {
            Items = list,
            TotalCount = total,
            IsSorted = true,
            IsFiltered = true,
            IsSearch = true,
            IsAdvanceSearch = true
        };
    }
}
