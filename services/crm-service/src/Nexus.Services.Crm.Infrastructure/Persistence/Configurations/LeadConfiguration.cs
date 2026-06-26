using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Services.Crm.Domain.Leads;

namespace Nexus.Services.Crm.Infrastructure.Persistence.Configurations;

public sealed class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("leads");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName).HasMaxLength(LeadConsts.FullNameMaxLength).IsRequired();
        builder.Property(x => x.CompanyName).HasMaxLength(LeadConsts.CompanyNameMaxLength);
        builder.Property(x => x.Title).HasMaxLength(LeadConsts.TitleMaxLength);
        builder.Property(x => x.Email).HasMaxLength(LeadConsts.EmailMaxLength);
        builder.Property(x => x.Phone).HasMaxLength(LeadConsts.PhoneMaxLength);
        builder.Property(x => x.Source).HasMaxLength(LeadConsts.SourceMaxLength);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Rating).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Address).HasMaxLength(LeadConsts.AddressMaxLength);
        builder.Property(x => x.City).HasMaxLength(LeadConsts.CityMaxLength);
        builder.Property(x => x.Country).HasMaxLength(LeadConsts.CountryMaxLength);
        builder.Property(x => x.ConcurrencyStamp).HasMaxLength(64).IsConcurrencyToken();

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.OwnerId });
    }
}
