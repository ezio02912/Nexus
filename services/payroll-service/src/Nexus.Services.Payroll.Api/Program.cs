using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore.DependencyInjection;
using Nexus.BuildingBlocks.Observability;
using Nexus.BuildingBlocks.Web.DependencyInjection;
using Nexus.Services.Payroll.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.AddNexusObservability("payroll-service");

var connectionString = builder.Configuration.GetConnectionString("PayrollDb")
    ?? "Host=localhost;Port=5432;Database=payroll_db;Username=nexus;Password=nexus_dev_password";

builder.Services.AddNexusWeb();
builder.Services.AddNexusEfCore<PayrollDbContext>(connectionString);

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { Service = "Nexus Payroll Service", Status = "Running" }));
app.MapGet("/health", async (PayrollDbContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return ok ? Results.Ok(new { Status = "Healthy" }) : Results.StatusCode(503);
});

MapCrud<PayrollPolicy>(app, "/api/payroll/policies", x => $"{x.PolicyCode} {x.Name} {x.CountryCode} {x.Status}");
MapCrud<SalaryComponent>(app, "/api/payroll/components", x => $"{x.ComponentCode} {x.Name} {x.ComponentType} {x.Formula} {x.Status}");
MapCrud<PayrollPeriod>(app, "/api/payroll/periods", x => $"{x.PeriodCode} {x.Month}/{x.Year} {x.Status}");
MapCrud<PayrollRun>(app, "/api/payroll/runs", x => $"{x.RunNo} {x.Status}");
MapCrud<PayrollLine>(app, "/api/payroll/lines", x => $"{x.EmployeeId} {x.PaymentStatus}");
MapCrud<PayrollLineComponent>(app, "/api/payroll/line-components", x => $"{x.FormulaResult}");
MapCrud<Payslip>(app, "/api/payroll/payslips", x => $"{x.PayslipNo} {x.Status}");
MapCrud<PayrollPayment>(app, "/api/payroll/payments", x => $"{x.PaymentNo} {x.PaymentMethod} {x.Status} {x.ReferenceNo}");

app.MapPost("/api/payroll/setup-vn-defaults", async (SetupDefaultsRequest input, PayrollDbContext db, CancellationToken ct) =>
{
    var now = DateTimeOffset.UtcNow;
    if (!await db.PayrollPolicies.AnyAsync(x => x.TenantId == input.TenantId && x.PolicyCode == "VN-DEFAULT", ct))
    {
        await db.PayrollPolicies.AddAsync(new PayrollPolicy
        {
            Id = Guid.NewGuid(),
            TenantId = input.TenantId,
            PolicyCode = "VN-DEFAULT",
            Name = "Chính sách lương Việt Nam mặc định",
            EffectiveFrom = new DateOnly(DateTime.UtcNow.Year, 1, 1),
            CreatedAt = now,
            UpdatedAt = now
        }, ct);
    }

    var components = new[]
    {
        ("BASE", "Lương cơ bản", "Earning", true, true, true, "BaseSalary"),
        ("ALLOWANCE", "Phụ cấp", "Allowance", true, false, true, "Manual"),
        ("DEDUCTION", "Khấu trừ", "Deduction", false, false, false, "Manual"),
        ("OVERTIME", "Tăng ca", "Earning", true, false, false, "OvertimeHours * Rate")
    };

    foreach (var (code, name, type, taxable, insurance, recurring, formula) in components)
    {
        if (await db.SalaryComponents.AnyAsync(x => x.TenantId == input.TenantId && x.ComponentCode == code, ct))
        {
            continue;
        }

        await db.SalaryComponents.AddAsync(new SalaryComponent
        {
            Id = Guid.NewGuid(),
            TenantId = input.TenantId,
            ComponentCode = code,
            Name = name,
            ComponentType = type,
            Taxable = taxable,
            InsuranceIncluded = insurance,
            Recurring = recurring,
            Formula = formula,
            DisplayOrder = Array.FindIndex(components, x => x.Item1 == code) + 1,
            CreatedAt = now,
            UpdatedAt = now
        }, ct);
    }

    await db.SaveChangesAsync(ct);
    return Results.Ok(new { Status = "Configured", input.TenantId });
});

app.MapPost("/api/payroll/runs/{id:guid}/calculate", async (Guid id, PayrollDbContext db, CancellationToken ct) =>
{
    var run = await db.PayrollRuns.SingleOrDefaultAsync(x => x.Id == id, ct);
    if (run is null)
    {
        return Results.NotFound();
    }

    var policy = await db.PayrollPolicies
        .Where(x => x.TenantId == run.TenantId && x.Status == "Active")
        .OrderByDescending(x => x.EffectiveFrom)
        .FirstOrDefaultAsync(ct);

    var lines = await db.PayrollLines.Where(x => x.PayrollRunId == run.Id && x.TenantId == run.TenantId).ToListAsync(ct);
    var now = DateTimeOffset.UtcNow;
    foreach (var line in lines)
    {
        var employeeInsuranceRate = (policy?.SocialInsuranceEmployeeRate ?? 8)
            + (policy?.HealthInsuranceEmployeeRate ?? 1.5m)
            + (policy?.UnemploymentInsuranceEmployeeRate ?? 1);
        var employerInsuranceRate = (policy?.SocialInsuranceEmployerRate ?? 17.5m)
            + (policy?.HealthInsuranceEmployerRate ?? 3)
            + (policy?.UnemploymentInsuranceEmployerRate ?? 1);

        line.GrossIncome = line.BaseSalary + line.TotalAllowance;
        line.InsuranceSalary = line.InsuranceSalary <= 0 ? line.BaseSalary : line.InsuranceSalary;
        line.EmployeeInsuranceAmount = Math.Round(line.InsuranceSalary * employeeInsuranceRate / 100, 0);
        line.EmployerInsuranceAmount = Math.Round(line.InsuranceSalary * employerInsuranceRate / 100, 0);
        line.PersonalDeduction = policy?.PersonalDeductionAmount ?? 11000000;
        line.TaxableIncome = Math.Max(0, line.GrossIncome - line.EmployeeInsuranceAmount - line.PersonalDeduction - line.DependentDeduction);
        line.PitAmount = CalculatePit(line.TaxableIncome);
        line.NetPay = line.GrossIncome - line.EmployeeInsuranceAmount - line.PitAmount - line.TotalDeduction;
        Touch(line, now);
    }

    run.TotalGross = lines.Sum(x => x.GrossIncome);
    run.TotalInsuranceEmployee = lines.Sum(x => x.EmployeeInsuranceAmount);
    run.TotalTaxableIncome = lines.Sum(x => x.TaxableIncome);
    run.TotalPit = lines.Sum(x => x.PitAmount);
    run.TotalNetPay = lines.Sum(x => x.NetPay);
    run.Status = "Calculated";
    run.CalculatedAt = now;
    Touch(run, now);

    await db.SaveChangesAsync(ct);
    return Results.Ok(run);
});

app.MapPost("/api/payroll/runs/{id:guid}/approve", async (Guid id, ApprovalRequest input, PayrollDbContext db, CancellationToken ct) =>
{
    var run = await db.PayrollRuns.SingleOrDefaultAsync(x => x.Id == id, ct);
    if (run is null)
    {
        return Results.NotFound();
    }

    var now = DateTimeOffset.UtcNow;
    run.Status = "Approved";
    run.ApprovedBy = input.ApproverId;
    run.ApprovedAt = now;
    Touch(run, now);
    await db.SaveChangesAsync(ct);
    return Results.Ok(run);
});

app.MapPost("/api/payroll/runs/{id:guid}/pay", async (Guid id, PayrollDbContext db, CancellationToken ct) =>
{
    var run = await db.PayrollRuns.SingleOrDefaultAsync(x => x.Id == id, ct);
    if (run is null)
    {
        return Results.NotFound();
    }

    var now = DateTimeOffset.UtcNow;
    var lines = await db.PayrollLines.Where(x => x.TenantId == run.TenantId && x.PayrollRunId == run.Id).ToListAsync(ct);
    foreach (var line in lines)
    {
        line.PaymentStatus = "Paid";
        Touch(line, now);
        await db.PayrollPayments.AddAsync(new PayrollPayment
        {
            Id = Guid.NewGuid(),
            TenantId = run.TenantId,
            PaymentNo = $"PAY-{now:yyyyMMddHHmmss}-{line.EmployeeId.ToString("N")[..6]}",
            PayrollRunId = run.Id,
            EmployeeId = line.EmployeeId,
            Amount = line.NetPay,
            Status = "Paid",
            PaidAt = now,
            ReferenceNo = run.RunNo,
            CreatedAt = now,
            UpdatedAt = now
        }, ct);
    }

    run.Status = "Paid";
    run.PaidAt = now;
    Touch(run, now);
    await db.SaveChangesAsync(ct);
    return Results.Ok(run);
});

app.MapPost("/api/payroll/runs/{id:guid}/publish-payslips", async (Guid id, PayrollDbContext db, CancellationToken ct) =>
{
    var run = await db.PayrollRuns.SingleOrDefaultAsync(x => x.Id == id, ct);
    if (run is null)
    {
        return Results.NotFound();
    }

    var now = DateTimeOffset.UtcNow;
    var existingLineIds = await db.Payslips
        .Where(x => x.TenantId == run.TenantId)
        .Select(x => x.PayrollLineId)
        .ToListAsync(ct);
    var lines = await db.PayrollLines
        .Where(x => x.TenantId == run.TenantId && x.PayrollRunId == run.Id && !existingLineIds.Contains(x.Id))
        .ToListAsync(ct);

    foreach (var line in lines)
    {
        await db.Payslips.AddAsync(new Payslip
        {
            Id = Guid.NewGuid(),
            TenantId = run.TenantId,
            PayslipNo = $"PS-{now:yyyyMMddHHmmss}-{line.EmployeeId.ToString("N")[..6]}",
            PayrollLineId = line.Id,
            EmployeeId = line.EmployeeId,
            Status = "Published",
            PublishedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        }, ct);
    }

    await db.SaveChangesAsync(ct);
    return Results.Ok(new { Published = lines.Count });
});

app.Run();

static void MapCrud<TEntity>(WebApplication app, string route, Func<TEntity, string> searchText)
    where TEntity : PayrollEntity, new()
{
    var group = app.MapGroup(route);

    group.MapGet("/", async (Guid tenantId, string? search, PayrollDbContext db, CancellationToken ct) =>
    {
        var items = await db.Set<TEntity>()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(ct);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            items = items.Where(x => searchText(x).Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return Results.Ok(items);
    });

    group.MapGet("/{id:guid}", async (Guid id, Guid tenantId, PayrollDbContext db, CancellationToken ct) =>
    {
        var item = await db.Set<TEntity>().SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);
        return item is null ? Results.NotFound() : Results.Ok(item);
    });

    group.MapPost("/", async (TEntity input, PayrollDbContext db, CancellationToken ct) =>
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

    group.MapDelete("/{id:guid}", async (Guid id, Guid tenantId, PayrollDbContext db, CancellationToken ct) =>
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

static decimal CalculatePit(decimal taxableIncome)
{
    if (taxableIncome <= 0)
    {
        return 0;
    }

    var brackets = new (decimal Limit, decimal Rate)[]
    {
        (5000000, 0.05m),
        (10000000, 0.10m),
        (18000000, 0.15m),
        (32000000, 0.20m),
        (52000000, 0.25m),
        (80000000, 0.30m),
        (decimal.MaxValue, 0.35m)
    };

    decimal remaining = taxableIncome;
    decimal previousLimit = 0;
    decimal tax = 0;
    foreach (var (limit, rate) in brackets)
    {
        var amount = Math.Min(remaining, limit - previousLimit);
        if (amount <= 0)
        {
            break;
        }

        tax += amount * rate;
        remaining -= amount;
        previousLimit = limit;
    }

    return Math.Round(tax, 0);
}

static void Touch(PayrollEntity entity, DateTimeOffset now)
{
    entity.UpdatedAt = now;
    entity.ConcurrencyStamp = Guid.NewGuid().ToString("N");
}

public sealed record SetupDefaultsRequest(Guid TenantId);
public sealed record ApprovalRequest(Guid? ApproverId);
