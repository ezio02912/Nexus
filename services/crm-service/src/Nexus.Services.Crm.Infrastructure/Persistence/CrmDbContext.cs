using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;
using Nexus.Services.Crm.Domain.Activities;
using Nexus.Services.Crm.Domain.Contacts;
using Nexus.Services.Crm.Domain.Contracts;
using Nexus.Services.Crm.Domain.Customers;
using Nexus.Services.Crm.Domain.Leads;
using Nexus.Services.Crm.Domain.Opportunities;
using Nexus.Services.Crm.Domain.PipelineStages;
using Nexus.Services.Crm.Domain.Quotations;

namespace Nexus.Services.Crm.Infrastructure.Persistence;

public sealed class CrmDbContext : NexusDbContext
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<QuotationLine> QuotationLines => Set<QuotationLine>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractLine> ContractLines => Set<ContractLine>();
    public DbSet<CrmActivity> Activities => Set<CrmActivity>();
    public DbSet<PipelineStage> PipelineStages => Set<PipelineStage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
