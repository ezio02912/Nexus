using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Services.Crm.Domain.Customers;

namespace Nexus.Services.Crm.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(CustomerConsts.CodeMaxLength).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(CustomerConsts.NameMaxLength).IsRequired();
        builder.Property(x => x.CustomerType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(CustomerConsts.EmailMaxLength);
        builder.Property(x => x.Phone).HasMaxLength(CustomerConsts.PhoneMaxLength);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.TaxCode).HasMaxLength(CustomerConsts.TaxCodeMaxLength);
        builder.Property(x => x.Website).HasMaxLength(CustomerConsts.WebsiteMaxLength);
        builder.Property(x => x.Industry).HasMaxLength(CustomerConsts.IndustryMaxLength);
        builder.Property(x => x.AddressLine1).HasMaxLength(CustomerConsts.AddressMaxLength);
        builder.Property(x => x.AddressLine2).HasMaxLength(CustomerConsts.AddressMaxLength);
        builder.Property(x => x.City).HasMaxLength(CustomerConsts.CityMaxLength);
        builder.Property(x => x.State).HasMaxLength(CustomerConsts.StateMaxLength);
        builder.Property(x => x.PostalCode).HasMaxLength(CustomerConsts.PostalCodeMaxLength);
        builder.Property(x => x.Country).HasMaxLength(CustomerConsts.CountryMaxLength);
        builder.Property(x => x.Source).HasMaxLength(CustomerConsts.SourceMaxLength);
        builder.Property(x => x.Rating).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.ConcurrencyStamp).HasMaxLength(64).IsConcurrencyToken();

        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Name });
        builder.HasIndex(x => new { x.TenantId, x.OwnerId });
    }
}
