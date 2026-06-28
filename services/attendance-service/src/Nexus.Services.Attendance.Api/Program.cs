using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore.DependencyInjection;
using Nexus.BuildingBlocks.Observability;
using Nexus.BuildingBlocks.Web.DependencyInjection;
using Nexus.Services.Attendance.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.AddNexusObservability("attendance-service");

var connectionString = builder.Configuration.GetConnectionString("AttendanceDb")
    ?? "Host=localhost;Port=5432;Database=attendance_db;Username=nexus;Password=nexus_dev_password";

builder.Services.AddNexusWeb();
builder.Services.AddNexusEfCore<AttendanceDbContext>(connectionString);

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { Service = "Nexus Attendance Service", Status = "Running" }));
app.MapGet("/health", async (AttendanceDbContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return ok ? Results.Ok(new { Status = "Healthy" }) : Results.StatusCode(503);
});

MapCrud<WorkCalendar>(app, "/api/attendance/work-calendars", x => $"{x.CalendarCode} {x.Name} {x.CountryCode} {x.WorkingDays} {x.Status}");
MapCrud<Holiday>(app, "/api/attendance/holidays", x => $"{x.HolidayDate} {x.Name} {x.HolidayType} {x.CountryCode}");
MapCrud<Shift>(app, "/api/attendance/shifts", x => $"{x.ShiftCode} {x.Name} {x.Status}");
MapCrud<ShiftAssignment>(app, "/api/attendance/shift-assignments", x => $"{x.AssignmentType} {x.WorkDate}");
MapCrud<AttendanceRecord>(app, "/api/attendance/records", x => $"{x.WorkDate} {x.Status} {x.CorrectionStatus} {x.CorrectionReason}");
MapCrud<LeaveType>(app, "/api/attendance/leave-types", x => $"{x.LeaveTypeCode} {x.Name} {x.Status}");
MapCrud<LeaveBalance>(app, "/api/attendance/leave-balances", x => $"{x.EmployeeId} {x.Year} {x.RemainingDays}");
MapCrud<LeaveRequest>(app, "/api/attendance/leave-requests", x => $"{x.RequestNo} {x.Status} {x.Reason} {x.RejectedReason}");
MapCrud<OvertimeRequest>(app, "/api/attendance/overtime-requests", x => $"{x.RequestNo} {x.WorkDate} {x.OvertimeType} {x.Status} {x.Reason}");

app.MapPost("/api/attendance/setup-vn-defaults", async (SetupDefaultsRequest input, AttendanceDbContext db, CancellationToken ct) =>
{
    var now = DateTimeOffset.UtcNow;
    if (!await db.WorkCalendars.AnyAsync(x => x.TenantId == input.TenantId && x.CalendarCode == "VN-STD", ct))
    {
        await db.WorkCalendars.AddAsync(new WorkCalendar
        {
            Id = Guid.NewGuid(),
            TenantId = input.TenantId,
            CalendarCode = "VN-STD",
            Name = "Lịch làm việc Việt Nam chuẩn",
            IsDefault = true,
            CreatedAt = now,
            UpdatedAt = now
        }, ct);
    }

    if (!await db.Shifts.AnyAsync(x => x.TenantId == input.TenantId && x.ShiftCode == "HC-0800-1700", ct))
    {
        await db.Shifts.AddAsync(new Shift
        {
            Id = Guid.NewGuid(),
            TenantId = input.TenantId,
            ShiftCode = "HC-0800-1700",
            Name = "Hành chính 08:00-17:00",
            CreatedAt = now,
            UpdatedAt = now
        }, ct);
    }

    if (!await db.LeaveTypes.AnyAsync(x => x.TenantId == input.TenantId && x.LeaveTypeCode == "ANNUAL", ct))
    {
        await db.LeaveTypes.AddAsync(new LeaveType
        {
            Id = Guid.NewGuid(),
            TenantId = input.TenantId,
            LeaveTypeCode = "ANNUAL",
            Name = "Phép năm",
            AnnualQuotaDays = 12,
            CarryForwardAllowed = true,
            MaxCarryForwardDays = 5,
            CreatedAt = now,
            UpdatedAt = now
        }, ct);
    }

    await db.SaveChangesAsync(ct);
    return Results.Ok(new { Status = "Configured", input.TenantId });
});

app.MapPost("/api/attendance/leave-requests/{id:guid}/approve", async (Guid id, ApprovalRequest input, AttendanceDbContext db, CancellationToken ct) =>
{
    var request = await db.LeaveRequests.SingleOrDefaultAsync(x => x.Id == id, ct);
    if (request is null)
    {
        return Results.NotFound();
    }

    var now = DateTimeOffset.UtcNow;
    if (!request.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
    {
        request.Status = "Approved";
        request.ApproverId = input.ApproverId;
        request.ApprovedAt = now;
        Touch(request, now);

        var balance = await db.LeaveBalances.SingleOrDefaultAsync(x =>
            x.TenantId == request.TenantId
            && x.EmployeeId == request.EmployeeId
            && x.Year == request.FromDate.Year
            && x.LeaveTypeId == request.LeaveTypeId, ct);

        if (balance is not null)
        {
            balance.PendingDays = Math.Max(0, balance.PendingDays - request.TotalDays);
            balance.UsedDays += request.TotalDays;
            balance.RemainingDays = balance.OpeningDays + balance.AccruedDays + balance.AdjustedDays - balance.UsedDays - balance.PendingDays;
            Touch(balance, now);
        }
    }

    await db.SaveChangesAsync(ct);
    return Results.Ok(request);
});

app.MapPost("/api/attendance/leave-requests/{id:guid}/reject", async (Guid id, RejectRequest input, AttendanceDbContext db, CancellationToken ct) =>
{
    var request = await db.LeaveRequests.SingleOrDefaultAsync(x => x.Id == id, ct);
    if (request is null)
    {
        return Results.NotFound();
    }

    var now = DateTimeOffset.UtcNow;
    request.Status = "Rejected";
    request.RejectedReason = input.Reason?.Trim() ?? string.Empty;
    Touch(request, now);
    await db.SaveChangesAsync(ct);
    return Results.Ok(request);
});

app.MapPost("/api/attendance/overtime-requests/{id:guid}/approve", async (Guid id, ApprovalRequest input, AttendanceDbContext db, CancellationToken ct) =>
{
    var request = await db.OvertimeRequests.SingleOrDefaultAsync(x => x.Id == id, ct);
    if (request is null)
    {
        return Results.NotFound();
    }

    var now = DateTimeOffset.UtcNow;
    request.Status = "Approved";
    request.ApproverId = input.ApproverId;
    request.ApprovedAt = now;
    Touch(request, now);
    await db.SaveChangesAsync(ct);
    return Results.Ok(request);
});

app.Run();

static void MapCrud<TEntity>(WebApplication app, string route, Func<TEntity, string> searchText)
    where TEntity : AttendanceEntity, new()
{
    var group = app.MapGroup(route);

    group.MapGet("/", async (Guid tenantId, string? search, AttendanceDbContext db, CancellationToken ct) =>
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

    group.MapGet("/{id:guid}", async (Guid id, Guid tenantId, AttendanceDbContext db, CancellationToken ct) =>
    {
        var item = await db.Set<TEntity>().SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);
        return item is null ? Results.NotFound() : Results.Ok(item);
    });

    group.MapPost("/", async (TEntity input, AttendanceDbContext db, CancellationToken ct) =>
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

    group.MapDelete("/{id:guid}", async (Guid id, Guid tenantId, AttendanceDbContext db, CancellationToken ct) =>
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

static void Touch(AttendanceEntity entity, DateTimeOffset now)
{
    entity.UpdatedAt = now;
    entity.ConcurrencyStamp = Guid.NewGuid().ToString("N");
}

public sealed record SetupDefaultsRequest(Guid TenantId);
public sealed record ApprovalRequest(Guid? ApproverId);
public sealed record RejectRequest(string? Reason);
