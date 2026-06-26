using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Services.Crm.Domain.Contacts;
using Nexus.Services.Crm.Domain.Customers;

namespace Nexus.Services.Crm.Infrastructure.Persistence.Configurations;

public sealed class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("contacts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName).HasMaxLength(ContactConsts.FullNameMaxLength).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(ContactConsts.EmailMaxLength);
        builder.Property(x => x.Phone).HasMaxLength(ContactConsts.PhoneMaxLength);
        builder.Property(x => x.Mobile).HasMaxLength(ContactConsts.MobileMaxLength);
        builder.Property(x => x.Position).HasMaxLength(ContactConsts.PositionMaxLength);
        builder.Property(x => x.Department).HasMaxLength(ContactConsts.DepartmentMaxLength);
        builder.Property(x => x.LinkedInUrl).HasMaxLength(ContactConsts.LinkedInUrlMaxLength);
        builder.Property(x => x.ConcurrencyStamp).HasMaxLength(64).IsConcurrencyToken();

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.TenantId, x.CustomerId });
        builder.HasIndex(x => new { x.TenantId, x.OwnerId });
    }
}
