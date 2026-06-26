using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore.Repositories;
using Nexus.Services.Crm.Domain.Enums;
using Nexus.Services.Crm.Domain.Quotations;

namespace Nexus.Services.Crm.Infrastructure.Repositories;

public sealed class EfCoreQuotationRepository : EfCoreRepository<Quotation, Guid>, IQuotationRepository
{
    public EfCoreQuotationRepository(NexusDbContext context) : base(context)
    {
    }

    public override async Task<Quotation?> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Set.Include(x => x.Lines).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public override async Task<Quotation> UpdateAsync(Quotation entity, CancellationToken cancellationToken = default)
    {
        Context.ChangeTracker.DetectChanges();

        foreach (var line in entity.Lines)
        {
            var entry = Context.Entry(line);
            if (entry.State == EntityState.Detached)
            {
                Context.Set<QuotationLine>().Add(line);
                entry = Context.Entry(line);
            }

            if (entry.State == EntityState.Modified
                && !await Context.Set<QuotationLine>().AnyAsync(x => x.Id == line.Id, cancellationToken))
            {
                entry.State = EntityState.Added;
            }
        }

        await Context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<Quotation?> FindByNoAsync(Guid tenantId, string quotationNo, CancellationToken cancellationToken = default)
    {
        var normalized = quotationNo.Trim().ToUpperInvariant();
        return await Set.SingleOrDefaultAsync(x => x.TenantId == tenantId && x.QuotationNo == normalized, cancellationToken);
    }

    public async Task<Quotation?> GetWithLinesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Set.Include(x => x.Lines).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Quotation>> GetListByTenantAsync(
        Guid tenantId,
        string? search,
        string? status,
        Guid? customerId,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default)
    {
        return await ApplyFilters(tenantId, search, status, customerId)
            .OrderByDescending(x => x.CreationTime)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetCountByTenantAsync(
        Guid tenantId,
        string? search,
        string? status,
        Guid? customerId,
        CancellationToken cancellationToken = default)
    {
        return await ApplyFilters(tenantId, search, status, customerId).LongCountAsync(cancellationToken);
    }

    public async Task<long> GetPendingApprovalCountAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await Set.LongCountAsync(
            x => x.TenantId == tenantId && x.Status == QuotationStatus.Draft,
            cancellationToken);
    }

    private IQueryable<Quotation> ApplyFilters(Guid tenantId, string? search, string? status, Guid? customerId)
    {
        var query = Set.Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.QuotationNo.ToLower().Contains(term)
                || (x.Subject != null && x.Subject.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<QuotationStatus>(status, true, out var statusEnum))
        {
            query = query.Where(x => x.Status == statusEnum);
        }

        if (customerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == customerId);
        }

        return query;
    }
}
