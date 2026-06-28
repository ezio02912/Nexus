using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;

namespace Nexus.Services.Attendance.Api.Persistence;

public sealed class AttendanceDbContext(DbContextOptions<AttendanceDbContext> options) : NexusDbContext(options)
{
    public DbSet<WorkCalendar> WorkCalendars => Set<WorkCalendar>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<ShiftAssignment> ShiftAssignments => Set<ShiftAssignment>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<OvertimeRequest> OvertimeRequests => Set<OvertimeRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WorkCalendar>().HasIndex(x => new { x.TenantId, x.CalendarCode }).IsUnique();
        modelBuilder.Entity<Shift>().HasIndex(x => new { x.TenantId, x.ShiftCode }).IsUnique();
        modelBuilder.Entity<LeaveType>().HasIndex(x => new { x.TenantId, x.LeaveTypeCode }).IsUnique();
        modelBuilder.Entity<LeaveBalance>().HasIndex(x => new { x.TenantId, x.EmployeeId, x.Year, x.LeaveTypeId }).IsUnique();
        modelBuilder.Entity<LeaveRequest>().HasIndex(x => new { x.TenantId, x.RequestNo }).IsUnique();
        modelBuilder.Entity<OvertimeRequest>().HasIndex(x => new { x.TenantId, x.RequestNo }).IsUnique();
    }
}

public abstract class AttendanceEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString("N");
}

public sealed class WorkCalendar : AttendanceEntity
{
    public string CalendarCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string CountryCode { get; set; } = "VN";
    public string WorkingDays { get; set; } = "Mon,Tue,Wed,Thu,Fri";
    public TimeOnly DefaultStartTime { get; set; } = new(8, 0);
    public TimeOnly DefaultEndTime { get; set; } = new(17, 0);
    public TimeOnly BreakStartTime { get; set; } = new(12, 0);
    public TimeOnly BreakEndTime { get; set; } = new(13, 0);
    public decimal StandardHoursPerDay { get; set; } = 8;
    public decimal StandardHoursPerWeek { get; set; } = 40;
    public int GraceLateMinutes { get; set; } = 5;
    public int GraceEarlyMinutes { get; set; } = 5;
    public bool IsDefault { get; set; }
    public string Status { get; set; } = "Active";
}

public sealed class Holiday : AttendanceEntity
{
    public DateOnly HolidayDate { get; set; }
    public string Name { get; set; } = "";
    public string HolidayType { get; set; } = "Public";
    public bool IsPaid { get; set; } = true;
    public string CountryCode { get; set; } = "VN";
    public int Year { get; set; }
    public string Source { get; set; } = "MasterData";
}

public sealed class Shift : AttendanceEntity
{
    public string ShiftCode { get; set; } = "";
    public string Name { get; set; } = "";
    public TimeOnly StartTime { get; set; } = new(8, 0);
    public TimeOnly EndTime { get; set; } = new(17, 0);
    public TimeOnly BreakStartTime { get; set; } = new(12, 0);
    public TimeOnly BreakEndTime { get; set; } = new(13, 0);
    public bool CrossDay { get; set; }
    public decimal StandardHours { get; set; } = 8;
    public int LateGraceMinutes { get; set; } = 5;
    public int EarlyLeaveGraceMinutes { get; set; } = 5;
    public string Status { get; set; } = "Active";
}

public sealed class ShiftAssignment : AttendanceEntity
{
    public Guid EmployeeId { get; set; }
    public Guid ShiftId { get; set; }
    public DateOnly WorkDate { get; set; }
    public string AssignmentType { get; set; } = "Manual";
    public Guid? DepartmentId { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
}

public sealed class AttendanceRecord : AttendanceEntity
{
    public Guid EmployeeId { get; set; }
    public Guid? DepartmentId { get; set; }
    public DateOnly WorkDate { get; set; }
    public Guid? ShiftId { get; set; }
    public DateTimeOffset? CheckInAt { get; set; }
    public DateTimeOffset? CheckOutAt { get; set; }
    public string CheckInSource { get; set; } = "";
    public string CheckOutSource { get; set; } = "";
    public decimal? CheckInLatitude { get; set; }
    public decimal? CheckInLongitude { get; set; }
    public decimal? CheckOutLatitude { get; set; }
    public decimal? CheckOutLongitude { get; set; }
    public int LateMinutes { get; set; }
    public int EarlyLeaveMinutes { get; set; }
    public int WorkedMinutes { get; set; }
    public int OvertimeMinutes { get; set; }
    public string Status { get; set; } = "Draft";
    public string CorrectionStatus { get; set; } = "None";
    public string CorrectionReason { get; set; } = "";
    public Guid? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
}

public sealed class LeaveType : AttendanceEntity
{
    public string LeaveTypeCode { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsPaid { get; set; } = true;
    public decimal AnnualQuotaDays { get; set; } = 12;
    public bool CarryForwardAllowed { get; set; }
    public decimal MaxCarryForwardDays { get; set; }
    public bool RequiresApproval { get; set; } = true;
    public string Status { get; set; } = "Active";
}

public sealed class LeaveBalance : AttendanceEntity
{
    public Guid EmployeeId { get; set; }
    public int Year { get; set; }
    public Guid LeaveTypeId { get; set; }
    public decimal OpeningDays { get; set; }
    public decimal AccruedDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal PendingDays { get; set; }
    public decimal AdjustedDays { get; set; }
    public decimal RemainingDays { get; set; }
}

public sealed class LeaveRequest : AttendanceEntity
{
    public string RequestNo { get; set; } = "";
    public Guid EmployeeId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public decimal TotalDays { get; set; }
    public string Reason { get; set; } = "";
    public string Status { get; set; } = "Pending";
    public Guid? ApproverId { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string RejectedReason { get; set; } = "";
    public Guid? AttachmentFileId { get; set; }
}

public sealed class OvertimeRequest : AttendanceEntity
{
    public string RequestNo { get; set; } = "";
    public Guid EmployeeId { get; set; }
    public DateOnly WorkDate { get; set; }
    public TimeOnly FromTime { get; set; }
    public TimeOnly ToTime { get; set; }
    public decimal TotalHours { get; set; }
    public string OvertimeType { get; set; } = "Weekday";
    public decimal RateMultiplier { get; set; } = 1.5m;
    public string Reason { get; set; } = "";
    public string Status { get; set; } = "Pending";
    public Guid? ApproverId { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
}
