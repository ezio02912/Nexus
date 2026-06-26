using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Nexus.BuildingBlocks.EntityFrameworkCore;
using Nexus.SharedKernel.Domain;

namespace Nexus.Services.Sales.Api.Persistence;

public sealed class SalesOrder : NexusEntity<Guid>
{
    private readonly List<SalesOrderLine> _lines = [];

    private SalesOrder()
    {
        OrderNo = string.Empty;
        Status = string.Empty;
    }

    public SalesOrder(Guid id, Guid tenantId, Guid customerId, string orderNo, IEnumerable<SalesOrderLineDraft> lines, DateTimeOffset createdAt)
    {
        Id = id;
        TenantId = tenantId;
        CustomerId = customerId;
        OrderNo = orderNo.Trim().ToUpperInvariant();
        Status = "Draft";
        CreatedAt = createdAt;

        foreach (var line in lines)
        {
            _lines.Add(new SalesOrderLine(Guid.NewGuid(), id, line.ProductCode, line.Description, line.Quantity, line.UnitPrice));
        }

        TotalAmount = _lines.Sum(x => x.LineAmount);
    }

    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string OrderNo { get; private set; }
    public string Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public IReadOnlyCollection<SalesOrderLine> Lines => _lines.AsReadOnly();
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public void Approve(DateTimeOffset now)
    {
        Status = "Approved";
        ApprovedAt = now;
    }

    public void Complete(DateTimeOffset now)
    {
        Status = "Completed";
        CompletedAt = now;
    }
}

public sealed record SalesOrderLineDraft(string ProductCode, string Description, decimal Quantity, decimal UnitPrice);

public sealed class SalesOrderLine : NexusEntity<Guid>
{
    private SalesOrderLine()
    {
        ProductCode = string.Empty;
        Description = string.Empty;
    }

    public SalesOrderLine(Guid id, Guid salesOrderId, string productCode, string description, decimal quantity, decimal unitPrice)
    {
        Id = id;
        SalesOrderId = salesOrderId;
        ProductCode = productCode.Trim().ToUpperInvariant();
        Description = description.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineAmount = quantity * unitPrice;
    }

    public Guid SalesOrderId { get; private set; }
    public string ProductCode { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineAmount { get; private set; }
}

public sealed class SalesDbContext : NexusDbContext
{
    public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options)
    {
    }

    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SalesOrder>(builder =>
        {
            builder.ToTable("sales_orders");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.OrderNo).HasMaxLength(64).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
            builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
            builder.HasIndex(x => new { x.TenantId, x.OrderNo }).IsUnique();
            builder.HasIndex(x => new { x.TenantId, x.CustomerId });

            var linesNavigation = builder.Metadata.FindNavigation(nameof(SalesOrder.Lines))!;
            linesNavigation.SetPropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(x => x.Lines)
                .WithOne()
                .HasForeignKey(x => x.SalesOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SalesOrderLine>(builder =>
        {
            builder.ToTable("sales_order_lines");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ProductCode).HasMaxLength(64).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Quantity).HasPrecision(18, 2);
            builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
            builder.Property(x => x.LineAmount).HasPrecision(18, 2);
            builder.HasIndex(x => x.SalesOrderId);
        });

        base.OnModelCreating(modelBuilder);
    }
}
