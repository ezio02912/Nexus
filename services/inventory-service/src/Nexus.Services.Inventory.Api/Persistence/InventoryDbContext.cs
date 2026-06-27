using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;
using Nexus.SharedKernel.Domain;

namespace Nexus.Services.Inventory.Api.Persistence;

public sealed class InventoryProduct : NexusEntity<Guid>
{
    private InventoryProduct()
    {
        ProductCode = string.Empty;
        ProductName = string.Empty;
        Unit = string.Empty;
        Category = string.Empty;
        Attributes = string.Empty;
        Variants = string.Empty;
    }

    public InventoryProduct(Guid id, Guid tenantId, string productCode, string productName, string unit, string? category, decimal price, decimal taxPercent, bool isActive, string? attributes, string? variants, DateTimeOffset now)
    {
        Id = id;
        TenantId = tenantId;
        ProductCode = StockBalance.NormalizeCode(productCode, 64);
        ProductName = productName.Trim();
        Unit = string.IsNullOrWhiteSpace(unit) ? "EA" : StockBalance.NormalizeCode(unit, 16);
        Category = category?.Trim() ?? string.Empty;
        Price = price;
        TaxPercent = taxPercent;
        IsActive = isActive;
        Attributes = attributes?.Trim() ?? string.Empty;
        Variants = variants?.Trim() ?? string.Empty;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid TenantId { get; private set; }
    public string ProductCode { get; private set; }
    public string ProductName { get; private set; }
    public string Unit { get; private set; }
    public string Category { get; private set; }
    public decimal Price { get; private set; }
    public decimal TaxPercent { get; private set; }
    public bool IsActive { get; private set; }
    public string Attributes { get; private set; }
    public string Variants { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(string productName, string unit, string? category, decimal price, decimal taxPercent, bool isActive, string? attributes, string? variants, DateTimeOffset now)
    {
        ProductName = productName.Trim();
        Unit = string.IsNullOrWhiteSpace(unit) ? "EA" : StockBalance.NormalizeCode(unit, 16);
        Category = category?.Trim() ?? string.Empty;
        Price = price;
        TaxPercent = taxPercent;
        IsActive = isActive;
        Attributes = attributes?.Trim() ?? string.Empty;
        Variants = variants?.Trim() ?? string.Empty;
        UpdatedAt = now;
    }
}

public sealed class Warehouse : NexusEntity<Guid>
{
    private Warehouse()
    {
        WarehouseCode = string.Empty;
        Name = string.Empty;
        Location = string.Empty;
    }

    public Warehouse(Guid id, Guid tenantId, string warehouseCode, string name, string? location, bool isActive, DateTimeOffset now)
    {
        Id = id;
        TenantId = tenantId;
        WarehouseCode = StockBalance.NormalizeCode(warehouseCode, 64);
        Name = name.Trim();
        Location = location?.Trim() ?? string.Empty;
        IsActive = isActive;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid TenantId { get; private set; }
    public string WarehouseCode { get; private set; }
    public string Name { get; private set; }
    public string Location { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(string name, string? location, bool isActive, DateTimeOffset now)
    {
        Name = name.Trim();
        Location = location?.Trim() ?? string.Empty;
        IsActive = isActive;
        UpdatedAt = now;
    }
}

public sealed class StockBalance : NexusEntity<Guid>
{
    private StockBalance()
    {
        WarehouseCode = string.Empty;
        ProductCode = string.Empty;
        ProductName = string.Empty;
    }

    public StockBalance(Guid id, Guid tenantId, string warehouseCode, string productCode, string productName)
    {
        Id = id;
        TenantId = tenantId;
        WarehouseCode = NormalizeCode(warehouseCode, 64);
        ProductCode = NormalizeCode(productCode, 64);
        ProductName = productName.Trim();
    }

    public Guid TenantId { get; private set; }
    public string WarehouseCode { get; private set; }
    public string ProductCode { get; private set; }
    public string ProductName { get; private set; }
    public decimal OnHandQuantity { get; private set; }
    public decimal ReservedQuantity { get; private set; }
    public decimal AvailableQuantity => OnHandQuantity - ReservedQuantity;
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Import(decimal quantity, DateTimeOffset now)
    {
        OnHandQuantity += EnsurePositive(quantity, nameof(quantity));
        UpdatedAt = now;
    }

    public bool CanReserve(decimal quantity) => AvailableQuantity >= quantity;

    public void Reserve(decimal quantity, DateTimeOffset now)
    {
        quantity = EnsurePositive(quantity, nameof(quantity));
        if (!CanReserve(quantity))
        {
            throw new InvalidOperationException($"Insufficient stock for product {ProductCode}.");
        }

        ReservedQuantity += quantity;
        UpdatedAt = now;
    }

    public void ShipReserved(decimal quantity, DateTimeOffset now)
    {
        quantity = EnsurePositive(quantity, nameof(quantity));
        ReservedQuantity = Math.Max(0, ReservedQuantity - quantity);
        OnHandQuantity -= quantity;
        UpdatedAt = now;
    }

    public static string NormalizeCode(string value, int maxLength)
    {
        var normalized = value.Trim().ToUpperInvariant();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static decimal EnsurePositive(decimal value, string name)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(name, "Quantity must be greater than zero.");
        }

        return value;
    }
}

public sealed class StockReservation : NexusEntity<Guid>
{
    private readonly List<StockReservationLine> _lines = [];

    private StockReservation()
    {
        SourceType = string.Empty;
        SourceNo = string.Empty;
        Status = string.Empty;
    }

    public StockReservation(Guid id, Guid tenantId, string sourceType, Guid sourceId, string sourceNo, IEnumerable<ReserveStockLineDto> lines, DateTimeOffset now)
    {
        Id = id;
        TenantId = tenantId;
        SourceType = StockBalance.NormalizeCode(sourceType, 32);
        SourceId = sourceId;
        SourceNo = sourceNo.Trim();
        Status = "Reserved";
        CreatedAt = now;

        foreach (var line in lines)
        {
            var warehouseCode = string.IsNullOrWhiteSpace(line.WarehouseCode) ? "MAIN" : line.WarehouseCode;
            _lines.Add(new StockReservationLine(Guid.NewGuid(), id, warehouseCode, line.ProductCode, line.Description, line.Quantity));
        }
    }

    public Guid TenantId { get; private set; }
    public string SourceType { get; private set; }
    public Guid SourceId { get; private set; }
    public string SourceNo { get; private set; }
    public string Status { get; private set; }
    public IReadOnlyCollection<StockReservationLine> Lines => _lines.AsReadOnly();
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ShippedAt { get; private set; }

    public void MarkShipped(DateTimeOffset now)
    {
        Status = "Shipped";
        ShippedAt = now;
    }
}

public sealed class StockReservationLine : NexusEntity<Guid>
{
    private StockReservationLine()
    {
        WarehouseCode = string.Empty;
        ProductCode = string.Empty;
        Description = string.Empty;
    }

    public StockReservationLine(Guid id, Guid reservationId, string warehouseCode, string productCode, string description, decimal quantity)
    {
        Id = id;
        ReservationId = reservationId;
        WarehouseCode = StockBalance.NormalizeCode(warehouseCode, 64);
        ProductCode = StockBalance.NormalizeCode(productCode, 64);
        Description = description.Trim();
        Quantity = quantity;
    }

    public Guid ReservationId { get; private set; }
    public string WarehouseCode { get; private set; }
    public string ProductCode { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
}

public sealed class StockMovement : NexusEntity<Guid>
{
    private StockMovement()
    {
        WarehouseCode = string.Empty;
        ProductCode = string.Empty;
        MovementType = string.Empty;
        SourceType = string.Empty;
        SourceNo = string.Empty;
    }

    public StockMovement(Guid id, Guid tenantId, string warehouseCode, string productCode, string movementType, decimal quantity, string sourceType, Guid sourceId, string sourceNo, DateTimeOffset occurredAt)
    {
        Id = id;
        TenantId = tenantId;
        WarehouseCode = StockBalance.NormalizeCode(warehouseCode, 64);
        ProductCode = StockBalance.NormalizeCode(productCode, 64);
        MovementType = StockBalance.NormalizeCode(movementType, 32);
        Quantity = quantity;
        SourceType = StockBalance.NormalizeCode(sourceType, 32);
        SourceId = sourceId;
        SourceNo = sourceNo.Trim();
        OccurredAt = occurredAt;
    }

    public Guid TenantId { get; private set; }
    public string WarehouseCode { get; private set; }
    public string ProductCode { get; private set; }
    public string MovementType { get; private set; }
    public decimal Quantity { get; private set; }
    public string SourceType { get; private set; }
    public Guid SourceId { get; private set; }
    public string SourceNo { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
}

public sealed class InventoryDbContext : NexusDbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    public DbSet<StockBalance> StockBalances => Set<StockBalance>();
    public DbSet<InventoryProduct> Products => Set<InventoryProduct>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();
    public DbSet<StockReservationLine> StockReservationLines => Set<StockReservationLine>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryProduct>(builder =>
        {
            builder.ToTable("products");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ProductCode).HasMaxLength(64).IsRequired();
            builder.Property(x => x.ProductName).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Unit).HasMaxLength(16).IsRequired();
            builder.Property(x => x.Category).HasMaxLength(128).IsRequired();
            builder.Property(x => x.Attributes).HasMaxLength(2048).IsRequired();
            builder.Property(x => x.Variants).HasMaxLength(2048).IsRequired();
            builder.Property(x => x.Price).HasPrecision(18, 2);
            builder.Property(x => x.TaxPercent).HasPrecision(5, 2);
            builder.HasIndex(x => new { x.TenantId, x.ProductCode }).IsUnique();
        });

        modelBuilder.Entity<Warehouse>(builder =>
        {
            builder.ToTable("warehouses");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.WarehouseCode).HasMaxLength(64).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Location).HasMaxLength(256).IsRequired();
            builder.HasIndex(x => new { x.TenantId, x.WarehouseCode }).IsUnique();
        });

        modelBuilder.Entity<StockBalance>(builder =>
        {
            builder.ToTable("stock_balances");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.WarehouseCode).HasMaxLength(64).IsRequired();
            builder.Property(x => x.ProductCode).HasMaxLength(64).IsRequired();
            builder.Property(x => x.ProductName).HasMaxLength(256).IsRequired();
            builder.Property(x => x.OnHandQuantity).HasPrecision(18, 2);
            builder.Property(x => x.ReservedQuantity).HasPrecision(18, 2);
            builder.Ignore(x => x.AvailableQuantity);
            builder.HasIndex(x => new { x.TenantId, x.WarehouseCode, x.ProductCode }).IsUnique();
        });

        modelBuilder.Entity<StockReservation>(builder =>
        {
            builder.ToTable("stock_reservations");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.SourceType).HasMaxLength(32).IsRequired();
            builder.Property(x => x.SourceNo).HasMaxLength(64).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
            builder.HasIndex(x => new { x.TenantId, x.SourceType, x.SourceId }).IsUnique();
            builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.Cascade);
            builder.Metadata.FindNavigation(nameof(StockReservation.Lines))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<StockReservationLine>(builder =>
        {
            builder.ToTable("stock_reservation_lines");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.WarehouseCode).HasMaxLength(64).IsRequired();
            builder.Property(x => x.ProductCode).HasMaxLength(64).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Quantity).HasPrecision(18, 2);
            builder.HasIndex(x => x.ReservationId);
        });

        modelBuilder.Entity<StockMovement>(builder =>
        {
            builder.ToTable("stock_movements");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.WarehouseCode).HasMaxLength(64).IsRequired();
            builder.Property(x => x.ProductCode).HasMaxLength(64).IsRequired();
            builder.Property(x => x.MovementType).HasMaxLength(32).IsRequired();
            builder.Property(x => x.Quantity).HasPrecision(18, 2);
            builder.Property(x => x.SourceType).HasMaxLength(32).IsRequired();
            builder.Property(x => x.SourceNo).HasMaxLength(64).IsRequired();
            builder.HasIndex(x => new { x.TenantId, x.ProductCode, x.OccurredAt });
            builder.HasIndex(x => new { x.TenantId, x.SourceType, x.SourceId });
        });

        base.OnModelCreating(modelBuilder);
    }
}

public sealed record ReserveStockLineDto(string WarehouseCode, string ProductCode, string Description, decimal Quantity);
