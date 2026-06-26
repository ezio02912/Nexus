using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Services.Crm.Domain.Contacts;
using Nexus.Services.Crm.Domain.Contracts;
using Nexus.Services.Crm.Domain.Customers;
using Nexus.Services.Crm.Domain.Opportunities;
using Nexus.Services.Crm.Domain.Quotations;

namespace Nexus.Services.Crm.Infrastructure.Persistence.Configurations;

public sealed class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("contracts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContractNo).HasMaxLength(ContractConsts.ContractNoMaxLength).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(ContractConsts.TitleMaxLength).IsRequired();
        builder.Property(x => x.ContractValue).HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasMaxLength(ContractConsts.CurrencyMaxLength).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.ConcurrencyStamp).HasMaxLength(64).IsConcurrencyToken();

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Quotation>()
            .WithMany()
            .HasForeignKey(x => x.QuotationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Opportunity>()
            .WithMany()
            .HasForeignKey(x => x.OpportunityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Contact>()
            .WithMany()
            .HasForeignKey(x => x.ContactId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.ContractNo }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.CustomerId });
        builder.HasIndex(x => new { x.TenantId, x.EndDate });

        var linesNavigation = builder.Metadata.FindNavigation(nameof(Contract.Lines))!;
        linesNavigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        linesNavigation.SetField("_lines");

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
