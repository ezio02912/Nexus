using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore.DependencyInjection;
using Nexus.BuildingBlocks.Observability;
using Nexus.BuildingBlocks.Web.DependencyInjection;
using Nexus.Services.Hrm.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.AddNexusObservability("hrm-service");

var connectionString = builder.Configuration.GetConnectionString("HrmDb")
    ?? "Host=localhost;Port=5432;Database=hrm_db;Username=nexus;Password=nexus_dev_password";

builder.Services.AddNexusWeb();
builder.Services.AddNexusEfCore<HrmDbContext>(connectionString);

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { Service = "Nexus HRM Service", Status = "Running" }));
app.MapGet("/health", async (HrmDbContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return ok ? Results.Ok(new { Status = "Healthy" }) : Results.StatusCode(503);
});

MapCrud<Employee>(app, "/api/hrm/employees", x => $"{x.EmployeeCode} {x.FullName} {x.DisplayName} {x.WorkEmail} {x.Phone} {x.EmploymentStatus}");
MapCrud<Department>(app, "/api/hrm/departments", x => $"{x.DepartmentCode} {x.Name} {x.CostCenterCode} {x.Location} {x.Status}");
MapCrud<Position>(app, "/api/hrm/positions", x => $"{x.PositionCode} {x.Name} {x.Level} {x.JobGrade} {x.Status}");
MapCrud<EmployeeContract>(app, "/api/hrm/contracts", x => $"{x.ContractNo} {x.ContractType} {x.Status} {x.WorkingLocation}");
MapCrud<EmployeeHistory>(app, "/api/hrm/histories", x => $"{x.ChangeType} {x.OldValue} {x.NewValue} {x.Reason}");
MapCrud<JobRequisition>(app, "/api/hrm/requisitions", x => $"{x.RequisitionNo} {x.Title} {x.Priority} {x.Status} {x.WorkLocation}");
MapCrud<Candidate>(app, "/api/hrm/candidates", x => $"{x.CandidateCode} {x.FullName} {x.Email} {x.Phone} {x.Source} {x.Tags}");
MapCrud<JobApplication>(app, "/api/hrm/applications", x => $"{x.Stage} {x.Status} {x.RejectReason}");
MapCrud<Interview>(app, "/api/hrm/interviews", x => $"{x.InterviewType} {x.Interviewers} {x.LocationOrLink} {x.Result} {x.Feedback}");
MapCrud<Offer>(app, "/api/hrm/offers", x => $"{x.OfferNo} {x.Status} {x.RejectReason}");
MapCrud<OnboardingChecklist>(app, "/api/hrm/onboarding-checklists", x => $"{x.ChecklistNo} {x.Status}");

app.MapPost("/api/hrm/contracts/{id:guid}/sign", async (Guid id, HrmDbContext db, CancellationToken ct) =>
{
    var contract = await db.EmployeeContracts.SingleOrDefaultAsync(x => x.Id == id, ct);
    if (contract is null)
    {
        return Results.NotFound();
    }

    var now = DateTimeOffset.UtcNow;
    contract.Status = "Signed";
    contract.SignedDate ??= DateOnly.FromDateTime(now.DateTime);
    Touch(contract, now);
    await db.SaveChangesAsync(ct);
    return Results.Ok(contract);
});

app.MapPost("/api/hrm/employees/{id:guid}/resign", async (Guid id, ResignEmployeeRequest input, HrmDbContext db, CancellationToken ct) =>
{
    var employee = await db.Employees.SingleOrDefaultAsync(x => x.Id == id, ct);
    if (employee is null)
    {
        return Results.NotFound();
    }

    var now = DateTimeOffset.UtcNow;
    var oldValue = employee.EmploymentStatus;
    employee.EmploymentStatus = "Resigned";
    employee.ResignDate = input.ResignDate;
    employee.ResignReason = input.ResignReason?.Trim() ?? string.Empty;
    Touch(employee, now);

    await db.EmployeeHistories.AddAsync(new EmployeeHistory
    {
        Id = Guid.NewGuid(),
        TenantId = employee.TenantId,
        EmployeeId = employee.Id,
        ChangeType = "Resign",
        EffectiveDate = input.ResignDate,
        OldValue = oldValue,
        NewValue = "Resigned",
        Reason = employee.ResignReason,
        CreatedAt = now,
        UpdatedAt = now
    }, ct);

    await db.SaveChangesAsync(ct);
    return Results.Ok(employee);
});

app.MapPost("/api/hrm/offers/{id:guid}/accept", async (Guid id, HrmDbContext db, CancellationToken ct) =>
{
    var offer = await db.Offers.SingleOrDefaultAsync(x => x.Id == id, ct);
    if (offer is null)
    {
        return Results.NotFound();
    }

    var application = await db.Applications.SingleOrDefaultAsync(x => x.Id == offer.ApplicationId && x.TenantId == offer.TenantId, ct);
    var candidate = application is null
        ? null
        : await db.Candidates.SingleOrDefaultAsync(x => x.Id == application.CandidateId && x.TenantId == offer.TenantId, ct);
    if (application is null || candidate is null)
    {
        return Results.BadRequest(new { Error = "Application or candidate not found." });
    }

    var now = DateTimeOffset.UtcNow;
    offer.Status = "Accepted";
    offer.AcceptedAt ??= now;
    Touch(offer, now);
    application.Stage = "Hired";
    application.Status = "Won";
    application.OfferSalary = offer.OfferedSalary;
    Touch(application, now);
    candidate.Status = "Hired";
    Touch(candidate, now);

    var employee = new Employee
    {
        Id = Guid.NewGuid(),
        TenantId = offer.TenantId,
        EmployeeCode = $"EMP-{now:yyyyMMddHHmmss}",
        FullName = candidate.FullName,
        DisplayName = candidate.FullName,
        PersonalEmail = candidate.Email,
        Phone = candidate.Phone,
        EmploymentStatus = "Draft",
        EmploymentType = "FullTime",
        JoinDate = offer.StartDate,
        BaseSalary = offer.OfferedSalary,
        SalaryCurrency = offer.Currency,
        Notes = $"Created from offer {offer.OfferNo}",
        CreatedAt = now,
        UpdatedAt = now
    };

    await db.Employees.AddAsync(employee, ct);
    await db.OnboardingChecklists.AddAsync(new OnboardingChecklist
    {
        Id = Guid.NewGuid(),
        TenantId = offer.TenantId,
        EmployeeId = employee.Id,
        OfferId = offer.Id,
        ChecklistNo = $"ONB-{now:yyyyMMddHHmmss}",
        Status = "Open",
        ItemsJson = "[\"Chuẩn bị hồ sơ\",\"Cấp tài khoản\",\"Ký hợp đồng\",\"Bàn giao thiết bị\"]",
        CreatedAt = now,
        UpdatedAt = now
    }, ct);

    await db.SaveChangesAsync(ct);
    return Results.Ok(employee);
});

app.Run();

static void MapCrud<TEntity>(WebApplication app, string route, Func<TEntity, string> searchText)
    where TEntity : HrmRecord, new()
{
    var group = app.MapGroup(route);

    group.MapGet("/", async (Guid tenantId, string? search, HrmDbContext db, CancellationToken ct) =>
    {
        var items = await db.Set<TEntity>()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(ct);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            items = items
                .Where(x => searchText(x).Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return Results.Ok(items);
    });

    group.MapGet("/{id:guid}", async (Guid id, Guid tenantId, HrmDbContext db, CancellationToken ct) =>
    {
        var item = await db.Set<TEntity>().SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);
        return item is null ? Results.NotFound() : Results.Ok(item);
    });

    group.MapPost("/", async (TEntity input, HrmDbContext db, CancellationToken ct) =>
    {
        var now = DateTimeOffset.UtcNow;
        var set = db.Set<TEntity>();
        TEntity? entity = input.Id == Guid.Empty
            ? null
            : await set.SingleOrDefaultAsync(x => x.Id == input.Id && x.TenantId == input.TenantId, ct);

        if (entity is null)
        {
            input.Id = input.Id == Guid.Empty ? Guid.NewGuid() : input.Id;
            input.CreatedAt = now;
            input.UpdatedAt = now;
            input.ConcurrencyStamp = Guid.NewGuid().ToString("N");
            await set.AddAsync(input, ct);
            entity = input;
        }
        else
        {
            db.Entry(entity).CurrentValues.SetValues(input);
            entity.UpdatedAt = now;
            entity.ConcurrencyStamp = Guid.NewGuid().ToString("N");
        }

        await db.SaveChangesAsync(ct);
        return Results.Ok(entity);
    });

    group.MapDelete("/{id:guid}", async (Guid id, Guid tenantId, HrmDbContext db, CancellationToken ct) =>
    {
        var item = await db.Set<TEntity>().SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);
        if (item is null)
        {
            return Results.NotFound();
        }

        db.Remove(item);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    });
}

static void Touch(HrmRecord record, DateTimeOffset now)
{
    record.UpdatedAt = now;
    record.ConcurrencyStamp = Guid.NewGuid().ToString("N");
}

public sealed record ResignEmployeeRequest(DateOnly ResignDate, string? ResignReason);
