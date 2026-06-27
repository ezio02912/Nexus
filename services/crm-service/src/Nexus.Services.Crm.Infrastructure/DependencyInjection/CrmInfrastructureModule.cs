using Microsoft.Extensions.DependencyInjection;
using Nexus.BuildingBlocks.EntityFrameworkCore.DependencyInjection;
using Nexus.Services.Crm.Contracts.Numbering;
using Nexus.Services.Crm.Domain.Activities;
using Nexus.Services.Crm.Domain.Contacts;
using Nexus.Services.Crm.Domain.Contracts;
using Nexus.Services.Crm.Domain.Customers;
using Nexus.Services.Crm.Domain.Leads;
using Nexus.Services.Crm.Domain.Opportunities;
using Nexus.Services.Crm.Domain.PipelineStages;
using Nexus.Services.Crm.Domain.Quotations;
using Nexus.Services.Crm.Infrastructure.Numbering;
using Nexus.Services.Crm.Infrastructure.Outbox;
using Nexus.Services.Crm.Infrastructure.Persistence;
using Nexus.Services.Crm.Infrastructure.Repositories;
using Nexus.SharedKernel.Events;

namespace Nexus.Services.Crm.Infrastructure.DependencyInjection;

public static class CrmInfrastructureModule
{
    public static IServiceCollection AddCrmInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddNexusEfCore<CrmDbContext>(connectionString, typeof(IIntegrationEvent).Assembly);

        services.AddScoped<ICustomerRepository, EfCoreCustomerRepository>();
        services.AddScoped<IContactRepository, EfCoreContactRepository>();
        services.AddScoped<ILeadRepository, EfCoreLeadRepository>();
        services.AddScoped<IOpportunityRepository, EfCoreOpportunityRepository>();
        services.AddScoped<IQuotationRepository, EfCoreQuotationRepository>();
        services.AddScoped<IContractRepository, EfCoreContractRepository>();
        services.AddScoped<IActivityRepository, EfCoreActivityRepository>();
        services.AddScoped<IPipelineStageRepository, EfCorePipelineStageRepository>();
        services.AddScoped<CrmEventPublisher>();
        services.AddHttpContextAccessor();
        services.AddHttpClient<INumberingClient, HttpNumberingClient>();

        return services;
    }
}
