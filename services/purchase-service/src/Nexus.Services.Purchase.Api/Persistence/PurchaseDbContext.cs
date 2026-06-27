using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Nexus.BuildingBlocks.EntityFrameworkCore;
using Nexus.SharedKernel.Domain;

namespace Nexus.Services.Purchase.Api.Persistence;

public sealed class Supplier : NexusEntity<Guid>
{
    private Supplier()
    {
        SupplierCode = string.Empty;
        Name = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
    }

    public Supplier(Guid id, Guid tenantId, string supplierCode, string name, string? email, string? phone, DateTimeOffset now)
    {
        Id = id;
        TenantId = tenantId;
        SupplierCode = NormalizeCode(supplierCode, 64);
        Name = name.Trim();
        Email = email?.Trim() ?? string.Empty;
        Phone = phone?.Trim() ?? string.Empty;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid TenantId { get; private set; }
    public string SupplierCode { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(string name, string? email, string? phone, DateTimeOffset now)
    {
        Name = name.Trim();
        Email = email?.Trim() ?? string.Empty;
        Phone = phone?.Trim() ?? string.Empty;
        UpdatedAt = now;
    }

    public static string NormalizeCode(string value, int maxLength)
    {
        var normalized = value.Trim().ToUpperInvariant();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}

public sealed class PurchaseOrder : NexusEntity<Guid>
{
    private readonly List<PurchaseOrderLine> _lines = [];

    private PurchaseOrder()
    {
        PurchaseOrderNo = string.Empty;
        SupplierCode = string.Empty;
        SupplierName = string.Empty;
        Status = string.Empty;
    }

    public PurchaseOrder(Guid id, Guid tenantId, string purchaseOrderNo, string supplierCode, string supplierName, IEnumerable<PurchaseOrderLineDraft> lines, DateTimeOffset now)
    {
        Id = id;
        TenantId = tenantId;
        PurchaseOrderNo = Supplier.NormalizeCode(purchaseOrderNo, 64);
        SupplierCode = Supplier.NormalizeCode(supplierCode, 64);
        SupplierName = supplierName.Trim();
        Status = "Draft";
        CreatedAt = now;

        foreach (var line in lines)
        {
            _lines.Add(new PurchaseOrderLine(Guid.NewGuid(), id, line.WarehouseCode, line.ProductCode, line.ProductName, line.Quantity, line.UnitCost));
        }

        TotalAmount = _lines.Sum(x => x.LineAmount);
    }

    public Guid TenantId { get; private set; }
    public string PurchaseOrderNo { get; private set; }
    public string SupplierCode { get; private set; }
    public string SupplierName { get; private set; }
    public string Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public IReadOnlyCollection<PurchaseOrderLine> Lines => _lines.AsReadOnly();
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public DateTimeOffset? ReceivedAt { get; private set; }

    public void Approve(DateTimeOffset now)
    {
        Status = "Approved";
        ApprovedAt = now;
    }

    public void MarkReceived(DateTimeOffset now)
    {
        Status = "Received";
        ReceivedAt = now;
    }
}

public sealed record PurchaseOrderLineDraft(string WarehouseCode, string ProductCode, string ProductName, decimal Quantity, decimal UnitCost);

public sealed class PurchaseOrderLine : NexusEntity<Guid>
{
    private PurchaseOrderLine()
    {
        WarehouseCode = string.Empty;
        ProductCode = string.Empty;
        ProductName = string.Empty;
    }

    public PurchaseOrderLine(Guid id, Guid purchaseOrderId, string warehouseCode, string productCode, string productName, decimal quantity, decimal unitCost)
    {
        Id = id;
        PurchaseOrderId = purchaseOrderId;
        WarehouseCode = string.IsNullOrWhiteSpace(warehouseCode) ? "MAIN" : Supplier.NormalizeCode(warehouseCode, 64);
        ProductCode = Supplier.NormalizeCode(productCode, 64);
        ProductName = productName.Trim();
        Quantity = quantity;
        UnitCost = unitCost;
        LineAmount = quantity * unitCost;
    }

    public Guid PurchaseOrderId { get; private set; }
    public string WarehouseCode { get; private set; }
    public string ProductCode { get; private set; }
    public string ProductName { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal LineAmount { get; private set; }
}

public sealed class GoodsReceipt : NexusEntity<Guid>
{
    private readonly List<GoodsReceiptLine> _lines = [];

    private GoodsReceipt()
    {
        ReceiptNo = string.Empty;
        PurchaseOrderNo = string.Empty;
    }

    public GoodsReceipt(Guid id, Guid tenantId, Guid purchaseOrderId, string purchaseOrderNo, string receiptNo, IEnumerable<PurchaseOrderLine> lines, DateTimeOffset now)
    {
        Id = id;
        TenantId = tenantId;
        PurchaseOrderId = purchaseOrderId;
        PurchaseOrderNo = purchaseOrderNo;
        ReceiptNo = Supplier.NormalizeCode(receiptNo, 64);
        ReceivedAt = now;

        foreach (var line in lines)
        {
            _lines.Add(new GoodsReceiptLine(Guid.NewGuid(), id, line.WarehouseCode, line.ProductCode, line.ProductName, line.Quantity, line.UnitCost));
        }
    }

    public Guid TenantId { get; private set; }
    public Guid PurchaseOrderId { get; private set; }
    public string PurchaseOrderNo { get; private set; }
    public string ReceiptNo { get; private set; }
    public IReadOnlyCollection<GoodsReceiptLine> Lines => _lines.AsReadOnly();
    public DateTimeOffset ReceivedAt { get; private set; }
}

public sealed class GoodsReceiptLine : NexusEntity<Guid>
{
    private GoodsReceiptLine()
    {
        WarehouseCode = string.Empty;
        ProductCode = string.Empty;
        ProductName = string.Empty;
    }

    public GoodsReceiptLine(Guid id, Guid goodsReceiptId, string warehouseCode, string productCode, string productName, decimal quantity, decimal unitCost)
    {
        Id = id;
        GoodsReceiptId = goodsReceiptId;
        WarehouseCode = warehouseCode;
        ProductCode = productCode;
        ProductName = productName;
        Quantity = quantity;
        UnitCost = unitCost;
    }

    public Guid GoodsReceiptId { get; private set; }
    public string WarehouseCode { get; private set; }
    public string ProductCode { get; private set; }
    public string ProductName { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
}

public sealed class PurchaseDbContext : NexusDbContext
{
    public PurchaseDbContext(DbContextOptions<PurchaseDbContext> options) : base(options)
    {
    }

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<GoodsReceiptLine> GoodsReceiptLines => Set<GoodsReceiptLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Supplier>(builder =>
        {
            builder.ToTable("suppliers");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.SupplierCode).HasMaxLength(64).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Phone).HasMaxLength(64).IsRequired();
            builder.HasIndex(x => new { x.TenantId, x.SupplierCode }).IsUnique();
        });

        modelBuilder.Entity<PurchaseOrder>(builder =>
        {
            builder.ToTable("purchase_orders");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PurchaseOrderNo).HasMaxLength(64).IsRequired();
            builder.Property(x => x.SupplierCode).HasMaxLength(64).IsRequired();
            builder.Property(x => x.SupplierName).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
            builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
            builder.HasIndex(x => new { x.TenantId, x.PurchaseOrderNo }).IsUnique();
            builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
            builder.Metadata.FindNavigation(nameof(PurchaseOrder.Lines))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<PurchaseOrderLine>(builder =>
        {
            builder.ToTable("purchase_order_lines");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.WarehouseCode).HasMaxLength(64).IsRequired();
            builder.Property(x => x.ProductCode).HasMaxLength(64).IsRequired();
            builder.Property(x => x.ProductName).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Quantity).HasPrecision(18, 2);
            builder.Property(x => x.UnitCost).HasPrecision(18, 2);
            builder.Property(x => x.LineAmount).HasPrecision(18, 2);
            builder.HasIndex(x => x.PurchaseOrderId);
        });

        modelBuilder.Entity<GoodsReceipt>(builder =>
        {
            builder.ToTable("goods_receipts");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PurchaseOrderNo).HasMaxLength(64).IsRequired();
            builder.Property(x => x.ReceiptNo).HasMaxLength(64).IsRequired();
            builder.HasIndex(x => new { x.TenantId, x.ReceiptNo }).IsUnique();
            builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.GoodsReceiptId).OnDelete(DeleteBehavior.Cascade);
            builder.Metadata.FindNavigation(nameof(GoodsReceipt.Lines))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<GoodsReceiptLine>(builder =>
        {
            builder.ToTable("goods_receipt_lines");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.WarehouseCode).HasMaxLength(64).IsRequired();
            builder.Property(x => x.ProductCode).HasMaxLength(64).IsRequired();
            builder.Property(x => x.ProductName).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Quantity).HasPrecision(18, 2);
            builder.Property(x => x.UnitCost).HasPrecision(18, 2);
            builder.HasIndex(x => x.GoodsReceiptId);
        });

        base.OnModelCreating(modelBuilder);
    }
}
