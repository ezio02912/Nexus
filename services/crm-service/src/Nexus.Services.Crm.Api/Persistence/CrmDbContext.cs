using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;
using Nexus.SharedKernel.Domain;

namespace Nexus.Services.Crm.Api.Persistence;

public sealed class Customer : NexusEntity<Guid>
{
    private Customer()
    {
        Code = string.Empty;
        Name = string.Empty;
        Status = string.Empty;
    }

    public Customer(Guid id, Guid tenantId, string code, string name, string? email, string? phone, DateTimeOffset createdAt)
    {
        Id = id;
        TenantId = tenantId;
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        Status = "Active";
        CreatedAt = createdAt;
    }

    public Guid TenantId { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}

public sealed class Lead : NexusEntity<Guid>
{
    private Lead()
    {
        FullName = string.Empty;
        Status = string.Empty;
    }

    public Lead(Guid id, Guid tenantId, string fullName, string? companyName, string? email, string? phone, string? source, DateTimeOffset createdAt)
    {
        Id = id;
        TenantId = tenantId;
        FullName = fullName.Trim();
        CompanyName = string.IsNullOrWhiteSpace(companyName) ? null : companyName.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        Source = string.IsNullOrWhiteSpace(source) ? null : source.Trim();
        Status = "New";
        CreatedAt = createdAt;
    }

    public Guid TenantId { get; private set; }
    public string FullName { get; private set; }
    public string? CompanyName { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Source { get; private set; }
    public string Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}

public sealed class Opportunity : NexusEntity<Guid>
{
    private Opportunity()
    {
        Name = string.Empty;
        Stage = string.Empty;
    }

    public Opportunity(Guid id, Guid tenantId, Guid? customerId, string name, decimal amount, DateOnly? expectedCloseDate, DateTimeOffset createdAt)
    {
        Id = id;
        TenantId = tenantId;
        CustomerId = customerId;
        Name = name.Trim();
        Stage = "Open";
        Amount = amount;
        ExpectedCloseDate = expectedCloseDate;
        CreatedAt = createdAt;
    }

    public Guid TenantId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public string Name { get; private set; }
    public string Stage { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly? ExpectedCloseDate { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}

public sealed class Quotation : NexusEntity<Guid>
{
    private Quotation()
    {
        QuotationNo = string.Empty;
        Status = string.Empty;
    }

    public Quotation(Guid id, Guid tenantId, Guid customerId, string quotationNo, decimal totalAmount, DateTimeOffset createdAt)
    {
        Id = id;
        TenantId = tenantId;
        CustomerId = customerId;
        QuotationNo = quotationNo.Trim().ToUpperInvariant();
        TotalAmount = totalAmount;
        Status = "Draft";
        CreatedAt = createdAt;
    }

    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string QuotationNo { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }

    public void Approve(DateTimeOffset now)
    {
        Status = "Approved";
        ApprovedAt = now;
    }
}

public sealed class Contract : NexusEntity<Guid>
{
    private Contract()
    {
        ContractNo = string.Empty;
        Title = string.Empty;
        Status = string.Empty;
    }

    public Contract(Guid id, Guid tenantId, Guid customerId, string contractNo, string title, DateTimeOffset createdAt)
    {
        Id = id;
        TenantId = tenantId;
        CustomerId = customerId;
        ContractNo = contractNo.Trim().ToUpperInvariant();
        Title = title.Trim();
        Status = "Draft";
        CreatedAt = createdAt;
    }

    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string ContractNo { get; private set; }
    public string Title { get; private set; }
    public string Status { get; private set; }
    public DateTimeOffset? SignedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public void Sign(DateTimeOffset now)
    {
        Status = "Signed";
        SignedAt = now;
    }
}

public sealed class CrmDbContext : NexusDbContext
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<Contract> Contracts => Set<Contract>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(builder =>
        {
            builder.ToTable("customers");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Email).HasMaxLength(256);
            builder.Property(x => x.Phone).HasMaxLength(64);
            builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
            builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            builder.HasIndex(x => new { x.TenantId, x.Name });
        });

        modelBuilder.Entity<Lead>(builder =>
        {
            builder.ToTable("leads");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.FullName).HasMaxLength(256).IsRequired();
            builder.Property(x => x.CompanyName).HasMaxLength(256);
            builder.Property(x => x.Email).HasMaxLength(256);
            builder.Property(x => x.Phone).HasMaxLength(64);
            builder.Property(x => x.Source).HasMaxLength(128);
            builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
            builder.HasIndex(x => x.TenantId);
        });

        modelBuilder.Entity<Opportunity>(builder =>
        {
            builder.ToTable("opportunities");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Stage).HasMaxLength(64).IsRequired();
            builder.Property(x => x.Amount).HasPrecision(18, 2);
            builder.HasIndex(x => x.TenantId);
        });

        modelBuilder.Entity<Quotation>(builder =>
        {
            builder.ToTable("quotations");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.QuotationNo).HasMaxLength(64).IsRequired();
            builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
            builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
            builder.HasIndex(x => new { x.TenantId, x.QuotationNo }).IsUnique();
        });

        modelBuilder.Entity<Contract>(builder =>
        {
            builder.ToTable("contracts");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ContractNo).HasMaxLength(64).IsRequired();
            builder.Property(x => x.Title).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
            builder.HasIndex(x => new { x.TenantId, x.ContractNo }).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}
