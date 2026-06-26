using Microsoft.Extensions.DependencyInjection;
using Nexus.Services.Crm.Application.Activities;
using Nexus.Services.Crm.Application.Contacts;
using Nexus.Services.Crm.Application.Customers;
using Nexus.Services.Crm.Application.Dashboard;
using Nexus.Services.Crm.Application.Leads;
using Nexus.Services.Crm.Application.Opportunities;
using Nexus.Services.Crm.Application.Quotations;
using Nexus.Services.Crm.Contracts.Activities;
using Nexus.Services.Crm.Contracts.Contacts;
using Nexus.Services.Crm.Contracts.Customers;
using Nexus.Services.Crm.Contracts.Dashboard;
using Nexus.Services.Crm.Contracts.Leads;
using Nexus.Services.Crm.Contracts.Opportunities;
using Nexus.Services.Crm.Contracts.Quotations;
using ContractAppServiceImpl = Nexus.Services.Crm.Application.Contracts.ContractAppService;
using IContractAppService = Nexus.Services.Crm.Contracts.Contracts.IContractAppService;

namespace Nexus.Services.Crm.Application.DependencyInjection;

public static class CrmApplicationModule
{
    public static IServiceCollection AddCrmApplication(this IServiceCollection services)
    {
        services.AddScoped<ICustomerAppService, CustomerAppService>();
        services.AddScoped<IContactAppService, ContactAppService>();
        services.AddScoped<ILeadAppService, LeadAppService>();
        services.AddScoped<IOpportunityAppService, OpportunityAppService>();
        services.AddScoped<IQuotationAppService, QuotationAppService>();
        services.AddScoped<IContractAppService, ContractAppServiceImpl>();
        services.AddScoped<IActivityAppService, ActivityAppService>();
        services.AddScoped<ICrmDashboardAppService, CrmDashboardAppService>();

        return services;
    }
}
