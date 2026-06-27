namespace Nexus.ApiContracts.Permissions;

public static class NexusPermissions
{
    public const string GroupName = "Nexus";

    public static class Tenants
    {
        public const string Default = GroupName + ".Tenants";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Activate = Default + ".Activate";
        public const string ManageSettings = Default + ".ManageSettings";
        public const string ManageModules = Default + ".ManageModules";
    }

    public static class Users
    {
        public const string Default = GroupName + ".Users";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ManageRoles = Default + ".ManageRoles";
    }

    public static class Permissions
    {
        public const string Default = GroupName + ".Permissions";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
    }

    public static class Audit
    {
        public const string Default = GroupName + ".Audit";
        public const string View = Default + ".View";
    }

    public static class Files
    {
        public const string Default = GroupName + ".Files";
        public const string View = Default + ".View";
        public const string Upload = Default + ".Upload";
        public const string Delete = Default + ".Delete";
    }

    public static class Notifications
    {
        public const string Default = GroupName + ".Notifications";
        public const string View = Default + ".View";
        public const string Send = Default + ".Send";
    }

    public static class Workflow
    {
        public const string Default = GroupName + ".Workflow";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
        public const string Approve = Default + ".Approve";
    }

    public static class Numbering
    {
        public const string Default = GroupName + ".Numbering";
        public const string View = Default + ".View";
        public const string Generate = Default + ".Generate";
    }

    public static class MasterData
    {
        public const string Default = GroupName + ".MasterData";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
    }

    public static class Crm
    {
        public const string Default = GroupName + ".Crm";

        // Legacy coarse permissions kept for backward compatibility with existing role grants.
        public const string CustomersLegacy = Default + ".Customers";
        public const string ContactsLegacy = Default + ".Contacts";
        public const string LeadsLegacy = Default + ".Leads";
        public const string OpportunitiesLegacy = Default + ".Opportunities";
        public const string QuotationsLegacy = Default + ".Quotations";
        public const string ContractsLegacy = Default + ".Contracts";
        public const string ActivitiesLegacy = Default + ".Activities";

        public static class Dashboard
        {
            public const string View = Default + ".Dashboard.View";
        }

        public static class Customers
        {
            public const string View = Default + ".Customers.View";
            public const string Create = Default + ".Customers.Create";
            public const string Edit = Default + ".Customers.Edit";
            public const string Delete = Default + ".Customers.Delete";
        }

        public static class Contacts
        {
            public const string View = Default + ".Contacts.View";
            public const string Create = Default + ".Contacts.Create";
            public const string Edit = Default + ".Contacts.Edit";
            public const string Delete = Default + ".Contacts.Delete";
        }

        public static class Leads
        {
            public const string View = Default + ".Leads.View";
            public const string Create = Default + ".Leads.Create";
            public const string Edit = Default + ".Leads.Edit";
            public const string Delete = Default + ".Leads.Delete";
        }

        public static class Opportunities
        {
            public const string View = Default + ".Opportunities.View";
            public const string Create = Default + ".Opportunities.Create";
            public const string Edit = Default + ".Opportunities.Edit";
            public const string Delete = Default + ".Opportunities.Delete";
        }

        public static class OpportunityBoard
        {
            public const string View = Default + ".OpportunityBoard.View";
            public const string Edit = Default + ".OpportunityBoard.Edit";
        }

        public static class Quotations
        {
            public const string View = Default + ".Quotations.View";
            public const string Create = Default + ".Quotations.Create";
            public const string Edit = Default + ".Quotations.Edit";
            public const string Delete = Default + ".Quotations.Delete";
            public const string Approve = Default + ".Quotations.Approve";
        }

        public static class Contracts
        {
            public const string View = Default + ".Contracts.View";
            public const string Create = Default + ".Contracts.Create";
            public const string Edit = Default + ".Contracts.Edit";
            public const string Delete = Default + ".Contracts.Delete";
            public const string Sign = Default + ".Contracts.Sign";
        }

        public static class Activities
        {
            public const string View = Default + ".Activities.View";
            public const string Create = Default + ".Activities.Create";
            public const string Edit = Default + ".Activities.Edit";
            public const string Delete = Default + ".Activities.Delete";
            public const string Complete = Default + ".Activities.Complete";
        }
    }

    public static class Sales
    {
        public const string Default = GroupName + ".Sales";

        public const string OrdersLegacy = Default + ".Orders";
        public const string ApproveOrdersLegacy = Default + ".ApproveOrders";
        public const string CompleteOrdersLegacy = Default + ".CompleteOrders";

        public static class Orders
        {
            public const string View = Default + ".Orders.View";
            public const string Create = Default + ".Orders.Create";
            public const string Edit = Default + ".Orders.Edit";
            public const string Delete = Default + ".Orders.Delete";
            public const string Approve = Default + ".Orders.Approve";
            public const string Complete = Default + ".Orders.Complete";
        }
    }

    public static class Inventory
    {
        public const string Default = GroupName + ".Inventory";

        public static class Stock
        {
            public const string View = Default + ".Stock.View";
            public const string Import = Default + ".Stock.Import";
            public const string Reserve = Default + ".Stock.Reserve";
            public const string Ship = Default + ".Stock.Ship";
            public const string Transfer = Default + ".Stock.Transfer";
        }

        public static class Products
        {
            public const string View = Default + ".Products.View";
            public const string Manage = Default + ".Products.Manage";
        }

        public static class Warehouses
        {
            public const string View = Default + ".Warehouses.View";
            public const string Manage = Default + ".Warehouses.Manage";
        }
    }

    public static class Purchase
    {
        public const string Default = GroupName + ".Purchase";

        public static class Suppliers
        {
            public const string View = Default + ".Suppliers.View";
            public const string Manage = Default + ".Suppliers.Manage";
        }

        public static class Orders
        {
            public const string View = Default + ".Orders.View";
            public const string Create = Default + ".Orders.Create";
            public const string Approve = Default + ".Orders.Approve";
            public const string Receive = Default + ".Orders.Receive";
        }
    }

    public static class TenantAdmin
    {
        public const string Default = GroupName + ".Tenant";

        public static class Users
        {
            public const string View = Default + ".Users.View";
            public const string Create = Default + ".Users.Create";
            public const string Edit = Default + ".Users.Edit";
            public const string Delete = Default + ".Users.Delete";
        }

        public static class Permissions
        {
            public const string View = Default + ".Permissions.View";
            public const string Manage = Default + ".Permissions.Manage";
        }

        public static class Settings
        {
            public const string View = Default + ".Settings.View";
            public const string Edit = Default + ".Settings.Edit";
        }
    }

    /// <summary>Permissions assignable inside a tenant workspace (web-tenant role editor).</summary>
    public static IReadOnlyList<string> TenantAssignable { get; } =
    [
        Crm.Dashboard.View,
        Crm.Customers.View, Crm.Customers.Create, Crm.Customers.Edit, Crm.Customers.Delete,
        Crm.Contacts.View, Crm.Contacts.Create, Crm.Contacts.Edit, Crm.Contacts.Delete,
        Crm.Leads.View, Crm.Leads.Create, Crm.Leads.Edit, Crm.Leads.Delete,
        Crm.Opportunities.View, Crm.Opportunities.Create, Crm.Opportunities.Edit, Crm.Opportunities.Delete,
        Crm.OpportunityBoard.View, Crm.OpportunityBoard.Edit,
        Crm.Quotations.View, Crm.Quotations.Create, Crm.Quotations.Edit, Crm.Quotations.Delete, Crm.Quotations.Approve,
        Crm.Contracts.View, Crm.Contracts.Create, Crm.Contracts.Edit, Crm.Contracts.Delete, Crm.Contracts.Sign,
        Crm.Activities.View, Crm.Activities.Create, Crm.Activities.Edit, Crm.Activities.Delete, Crm.Activities.Complete,
        Sales.Orders.View, Sales.Orders.Create, Sales.Orders.Edit, Sales.Orders.Delete, Sales.Orders.Approve, Sales.Orders.Complete,
        Purchase.Suppliers.View, Purchase.Suppliers.Manage,
        Purchase.Orders.View, Purchase.Orders.Create, Purchase.Orders.Approve, Purchase.Orders.Receive,
        Inventory.Stock.View, Inventory.Stock.Import, Inventory.Stock.Reserve, Inventory.Stock.Ship, Inventory.Stock.Transfer,
        Inventory.Products.View, Inventory.Products.Manage,
        Inventory.Warehouses.View, Inventory.Warehouses.Manage,
        Files.View, Files.Upload, Files.Delete,
        Workflow.View, Workflow.Approve,
        TenantAdmin.Users.View, TenantAdmin.Users.Create, TenantAdmin.Users.Edit, TenantAdmin.Users.Delete,
        TenantAdmin.Permissions.View, TenantAdmin.Permissions.Manage,
        TenantAdmin.Settings.View, TenantAdmin.Settings.Edit
    ];

    /// <summary>
    /// Returns the full catalog of permission names, used to seed the permission service.
    /// </summary>
    public static IReadOnlyList<string> All { get; } =
    [
        Tenants.Create, Tenants.Edit, Tenants.Delete, Tenants.Activate, Tenants.ManageSettings, Tenants.ManageModules,
        Users.Create, Users.Edit, Users.Delete, Users.ManageRoles,
        Permissions.View, Permissions.Manage,
        Audit.View,
        Files.View, Files.Upload, Files.Delete,
        Notifications.View, Notifications.Send,
        Workflow.View, Workflow.Manage, Workflow.Approve,
        Numbering.View, Numbering.Generate,
        MasterData.View, MasterData.Manage,
        ..TenantAssignable,
        Crm.CustomersLegacy, Crm.ContactsLegacy, Crm.LeadsLegacy, Crm.OpportunitiesLegacy,
        Crm.QuotationsLegacy, Crm.ContractsLegacy, Crm.ActivitiesLegacy,
        Sales.OrdersLegacy, Sales.ApproveOrdersLegacy, Sales.CompleteOrdersLegacy
    ];
}
