using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Services.Crm.Domain.Contracts;

namespace Nexus.Services.Crm.Infrastructure.Persistence.Configurations;

public sealed class ContractLineConfiguration : IEntityTypeConfiguration<ContractLine>
{
    public void Configure(EntityTypeBuilder<ContractLine> builder)
    {
        builder.ToTable("contract_lines");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductCode).HasMaxLength(ContractConsts.ProductCodeMaxLength).IsRequired();
        builder.Property(x => x.ProductName).HasMaxLength(ContractConsts.ProductNameMaxLength).IsRequired();
        builder.Property(x => x.Unit).HasMaxLength(ContractConsts.UnitMaxLength).IsRequired();
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.DiscountPercent).HasPrecision(18, 2);
        builder.Property(x => x.TaxPercent).HasPrecision(18, 2);
        builder.Property(x => x.LineTotal).HasPrecision(18, 2);

        builder.HasIndex(x => new { x.TenantId, x.ContractId });
        builder.HasIndex(x => new { x.ContractId, x.LineNo }).IsUnique();
    }
}
