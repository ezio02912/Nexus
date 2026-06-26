namespace Nexus.ApiContracts.Permissions;

/// <summary>
/// Maps legacy coarse permissions (e.g. Nexus.Crm.Customers) to granular tenant UI/API permissions.
/// </summary>
public static class NexusPermissionLegacy
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> LegacyToGranular =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [NexusPermissions.Crm.CustomersLegacy] =
            [
                NexusPermissions.Crm.Customers.View,
                NexusPermissions.Crm.Customers.Create,
                NexusPermissions.Crm.Customers.Edit,
                NexusPermissions.Crm.Customers.Delete
            ],
            [NexusPermissions.Crm.ContactsLegacy] =
            [
                NexusPermissions.Crm.Contacts.View,
                NexusPermissions.Crm.Contacts.Create,
                NexusPermissions.Crm.Contacts.Edit,
                NexusPermissions.Crm.Contacts.Delete
            ],
            [NexusPermissions.Crm.LeadsLegacy] =
            [
                NexusPermissions.Crm.Leads.View,
                NexusPermissions.Crm.Leads.Create,
                NexusPermissions.Crm.Leads.Edit,
                NexusPermissions.Crm.Leads.Delete
            ],
            [NexusPermissions.Crm.OpportunitiesLegacy] =
            [
                NexusPermissions.Crm.Opportunities.View,
                NexusPermissions.Crm.Opportunities.Create,
                NexusPermissions.Crm.Opportunities.Edit,
                NexusPermissions.Crm.Opportunities.Delete
            ],
            [NexusPermissions.Crm.QuotationsLegacy] =
            [
                NexusPermissions.Crm.Quotations.View,
                NexusPermissions.Crm.Quotations.Create,
                NexusPermissions.Crm.Quotations.Edit,
                NexusPermissions.Crm.Quotations.Delete
            ],
            [NexusPermissions.Crm.ContractsLegacy] =
            [
                NexusPermissions.Crm.Contracts.View,
                NexusPermissions.Crm.Contracts.Create,
                NexusPermissions.Crm.Contracts.Edit,
                NexusPermissions.Crm.Contracts.Delete
            ],
            [NexusPermissions.Crm.ActivitiesLegacy] =
            [
                NexusPermissions.Crm.Activities.View,
                NexusPermissions.Crm.Activities.Create,
                NexusPermissions.Crm.Activities.Edit,
                NexusPermissions.Crm.Activities.Delete
            ],
            [NexusPermissions.Sales.OrdersLegacy] =
            [
                NexusPermissions.Sales.Orders.View,
                NexusPermissions.Sales.Orders.Create,
                NexusPermissions.Sales.Orders.Edit,
                NexusPermissions.Sales.Orders.Delete,
                NexusPermissions.Sales.Orders.Approve,
                NexusPermissions.Sales.Orders.Complete
            ],
            [NexusPermissions.Sales.ApproveOrdersLegacy] = [NexusPermissions.Sales.Orders.Approve],
            [NexusPermissions.Sales.CompleteOrdersLegacy] = [NexusPermissions.Sales.Orders.Complete]
        };

    public static bool IsGranted(IReadOnlyCollection<string> grantedPermissions, string permission)
    {
        if (grantedPermissions.Contains("*", StringComparer.Ordinal))
        {
            return true;
        }

        if (grantedPermissions.Contains(permission, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        foreach (var (legacy, granular) in LegacyToGranular)
        {
            if (!grantedPermissions.Contains(legacy, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            if (granular.Contains(permission, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static HashSet<string> Expand(IEnumerable<string> grantedPermissions)
    {
        var expanded = new HashSet<string>(grantedPermissions, StringComparer.OrdinalIgnoreCase);
        foreach (var permission in grantedPermissions)
        {
            if (!LegacyToGranular.TryGetValue(permission, out var granular))
            {
                continue;
            }

            foreach (var item in granular)
            {
                expanded.Add(item);
            }
        }

        return expanded;
    }
}
