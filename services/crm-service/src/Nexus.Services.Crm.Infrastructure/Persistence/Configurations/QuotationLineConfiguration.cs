using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Services.Crm.Domain.Quotations;

namespace Nexus.Services.Crm.Infrastructure.Persistence.Configurations;

public sealed class QuotationLineConfiguration : IEntityTypeConfiguration<QuotationLine>
{
    public void Configure(EntityTypeBuilder<QuotationLine> builder)
    {
        builder.ToTable("quotation_lines");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductCode).HasMaxLength(QuotationConsts.ProductCodeMaxLength).IsRequired();
        builder.Property(x => x.ProductName).HasMaxLength(QuotationConsts.ProductNameMaxLength).IsRequired();
        builder.Property(x => x.Unit).HasMaxLength(QuotationConsts.UnitMaxLength).IsRequired();
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.DiscountPercent).HasPrecision(18, 2);
        builder.Property(x => x.TaxPercent).HasPrecision(18, 2);
        builder.Property(x => x.LineTotal).HasPrecision(18, 2);

        builder.HasIndex(x => new { x.TenantId, x.QuotationId });
        builder.HasIndex(x => new { x.QuotationId, x.LineNo }).IsUnique();
    }
}
