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
        InventoryReservationStatus = string.Empty;
        DeliveryStatus = string.Empty;
    }

    public SalesOrder(
        Guid id,
        Guid tenantId,
        Guid customerId,
        string orderNo,
        string? sourceType,
        Guid? sourceId,
        string? sourceNo,
        IEnumerable<SalesOrderLineDraft> lines,
        DateTimeOffset createdAt)
    {
        Id = id;
        TenantId = tenantId;
        CustomerId = customerId;
        OrderNo = orderNo.Trim().ToUpperInvariant();
        SourceType = NormalizeOptional(sourceType, 32);
        SourceId = sourceId;
        SourceNo = NormalizeOptional(sourceNo, 64);
        Status = "Draft";
        InventoryReservationStatus = "Pending";
        DeliveryStatus = "Pending";
        CreatedAt = createdAt;

        foreach (var line in lines)
        {
            _lines.Add(new SalesOrderLine(Guid.NewGuid(), id, line.ProductCode, line.Description, line.Quantity, line.UnitPrice, line.DiscountPercent, line.TaxPercent));
        }

        Subtotal = _lines.Sum(x => x.Subtotal);
        DiscountAmount = _lines.Sum(x => x.DiscountAmount);
        TaxAmount = _lines.Sum(x => x.TaxAmount);
        TotalAmount = _lines.Sum(x => x.LineAmount);
    }

    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string OrderNo { get; private set; }
    public string? SourceType { get; private set; }
    public Guid? SourceId { get; private set; }
    public string? SourceNo { get; private set; }
    public string Status { get; private set; }
    public string InventoryReservationStatus { get; private set; }
    public string DeliveryStatus { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public IReadOnlyCollection<SalesOrderLine> Lines => _lines.AsReadOnly();
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public DateTimeOffset? ReservedAt { get; private set; }
    public DateTimeOffset? DeliveredAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public void Approve(DateTimeOffset now)
    {
        Status = "Approved";
        DeliveryStatus = "Ready";
        ApprovedAt = now;
    }

    public void MarkInventoryReserved(DateTimeOffset now)
    {
        InventoryReservationStatus = "Reserved";
        ReservedAt = now;
    }

    public void MarkDelivered(DateTimeOffset now)
    {
        DeliveryStatus = "Delivered";
        DeliveredAt = now;
    }

    public void Complete(DateTimeOffset now)
    {
        Status = "Completed";
        if (DeliveryStatus != "Delivered")
        {
            MarkDelivered(now);
        }

        CompletedAt = now;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}

public sealed record SalesOrderLineDraft(string ProductCode, string Description, decimal Quantity, decimal UnitPrice, decimal DiscountPercent, decimal TaxPercent);

public sealed class SalesOrderLine : NexusEntity<Guid>
{
    private SalesOrderLine()
    {
        ProductCode = string.Empty;
        Description = string.Empty;
    }

    public SalesOrderLine(Guid id, Guid salesOrderId, string productCode, string description, decimal quantity, decimal unitPrice, decimal discountPercent, decimal taxPercent)
    {
        Id = id;
        SalesOrderId = salesOrderId;
        ProductCode = productCode.Trim().ToUpperInvariant();
        Description = description.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountPercent = Math.Clamp(discountPercent, 0m, 100m);
        TaxPercent = Math.Max(0m, taxPercent);
        Subtotal = quantity * unitPrice;
        DiscountAmount = decimal.Round(Subtotal * DiscountPercent / 100, 2, MidpointRounding.AwayFromZero);
        TaxAmount = decimal.Round((Subtotal - DiscountAmount) * TaxPercent / 100, 2, MidpointRounding.AwayFromZero);
        LineAmount = Subtotal - DiscountAmount + TaxAmount;
    }

    public Guid SalesOrderId { get; private set; }
    public string ProductCode { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountPercent { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxPercent { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal Subtotal { get; private set; }
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
            builder.Property(x => x.SourceType).HasMaxLength(32);
            builder.Property(x => x.SourceNo).HasMaxLength(64);
            builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
            builder.Property(x => x.InventoryReservationStatus).HasMaxLength(32).IsRequired();
            builder.Property(x => x.DeliveryStatus).HasMaxLength(32).IsRequired();
            builder.Property(x => x.Subtotal).HasPrecision(18, 2);
            builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            builder.Property(x => x.TaxAmount).HasPrecision(18, 2);
            builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
            builder.HasIndex(x => new { x.TenantId, x.OrderNo }).IsUnique();
            builder.HasIndex(x => new { x.TenantId, x.CustomerId });
            builder.HasIndex(x => new { x.TenantId, x.SourceType, x.SourceId });

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
            builder.Property(x => x.DiscountPercent).HasPrecision(5, 2);
            builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            builder.Property(x => x.TaxPercent).HasPrecision(5, 2);
            builder.Property(x => x.TaxAmount).HasPrecision(18, 2);
            builder.Property(x => x.Subtotal).HasPrecision(18, 2);
            builder.Property(x => x.LineAmount).HasPrecision(18, 2);
            builder.HasIndex(x => x.SalesOrderId);
        });

        base.OnModelCreating(modelBuilder);
    }
}
