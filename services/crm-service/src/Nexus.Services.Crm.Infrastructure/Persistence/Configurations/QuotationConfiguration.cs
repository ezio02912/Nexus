using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Services.Crm.Domain.Contacts;
using Nexus.Services.Crm.Domain.Customers;
using Nexus.Services.Crm.Domain.Opportunities;
using Nexus.Services.Crm.Domain.Quotations;

namespace Nexus.Services.Crm.Infrastructure.Persistence.Configurations;

public sealed class QuotationConfiguration : IEntityTypeConfiguration<Quotation>
{
    public void Configure(EntityTypeBuilder<Quotation> builder)
    {
        builder.ToTable("quotations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuotationNo).HasMaxLength(QuotationConsts.QuotationNoMaxLength).IsRequired();
        builder.Property(x => x.Subject).HasMaxLength(QuotationConsts.SubjectMaxLength);
        builder.Property(x => x.Subtotal).HasPrecision(18, 2);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        builder.Property(x => x.DiscountPercent).HasPrecision(18, 2);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasMaxLength(QuotationConsts.CurrencyMaxLength).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.ConcurrencyStamp).HasMaxLength(64).IsConcurrencyToken();

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Opportunity>()
            .WithMany()
            .HasForeignKey(x => x.OpportunityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Contact>()
            .WithMany()
            .HasForeignKey(x => x.ContactId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.QuotationNo }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.CustomerId });

        var linesNavigation = builder.Metadata.FindNavigation(nameof(Quotation.Lines))!;
        linesNavigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        linesNavigation.SetField("_lines");

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.QuotationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
