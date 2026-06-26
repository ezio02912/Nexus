using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Services.Crm.Domain.Contacts;
using Nexus.Services.Crm.Domain.Customers;
using Nexus.Services.Crm.Domain.Leads;
using Nexus.Services.Crm.Domain.Opportunities;

namespace Nexus.Services.Crm.Infrastructure.Persistence.Configurations;

public sealed class OpportunityConfiguration : IEntityTypeConfiguration<Opportunity>
{
    public void Configure(EntityTypeBuilder<Opportunity> builder)
    {
        builder.ToTable("opportunities");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(OpportunityConsts.NameMaxLength).IsRequired();
        builder.Property(x => x.Stage).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasMaxLength(OpportunityConsts.CurrencyMaxLength).IsRequired();
        builder.Property(x => x.Source).HasMaxLength(OpportunityConsts.SourceMaxLength);
        builder.Property(x => x.Competitor).HasMaxLength(OpportunityConsts.CompetitorMaxLength);
        builder.Property(x => x.NextStep).HasMaxLength(OpportunityConsts.NextStepMaxLength);
        builder.Property(x => x.ConcurrencyStamp).HasMaxLength(64).IsConcurrencyToken();

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Lead>()
            .WithMany()
            .HasForeignKey(x => x.LeadId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Contact>()
            .WithMany()
            .HasForeignKey(x => x.ContactId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.TenantId, x.Stage });
        builder.HasIndex(x => new { x.TenantId, x.CustomerId });
        builder.HasIndex(x => new { x.TenantId, x.OwnerId });
    }
}
