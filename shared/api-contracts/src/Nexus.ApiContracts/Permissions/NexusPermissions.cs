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

    public static class Crm
    {
        public const string Default = GroupName + ".Crm";
        public const string Customers = Default + ".Customers";
        public const string Leads = Default + ".Leads";
        public const string Opportunities = Default + ".Opportunities";
        public const string Quotations = Default + ".Quotations";
        public const string Contracts = Default + ".Contracts";
    }

    public static class Sales
    {
        public const string Default = GroupName + ".Sales";
        public const string Orders = Default + ".Orders";
        public const string ApproveOrders = Default + ".ApproveOrders";
        public const string CompleteOrders = Default + ".CompleteOrders";
    }

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
        Crm.Customers, Crm.Leads, Crm.Opportunities, Crm.Quotations, Crm.Contracts,
        Sales.Orders, Sales.ApproveOrders, Sales.CompleteOrders
    ];
}
