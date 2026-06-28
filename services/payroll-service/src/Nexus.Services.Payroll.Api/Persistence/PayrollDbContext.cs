using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;

namespace Nexus.Services.Payroll.Api.Persistence;

public sealed class PayrollDbContext(DbContextOptions<PayrollDbContext> options) : NexusDbContext(options)
{
    public DbSet<PayrollPolicy> PayrollPolicies => Set<PayrollPolicy>();
    public DbSet<SalaryComponent> SalaryComponents => Set<SalaryComponent>();
    public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();
    public DbSet<PayrollRun> PayrollRuns => Set<PayrollRun>();
    public DbSet<PayrollLine> PayrollLines => Set<PayrollLine>();
    public DbSet<PayrollLineComponent> PayrollLineComponents => Set<PayrollLineComponent>();
    public DbSet<Payslip> Payslips => Set<Payslip>();
    public DbSet<PayrollPayment> PayrollPayments => Set<PayrollPayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PayrollPolicy>().HasIndex(x => new { x.TenantId, x.PolicyCode }).IsUnique();
        modelBuilder.Entity<SalaryComponent>().HasIndex(x => new { x.TenantId, x.ComponentCode }).IsUnique();
        modelBuilder.Entity<PayrollPeriod>().HasIndex(x => new { x.TenantId, x.PeriodCode }).IsUnique();
        modelBuilder.Entity<PayrollRun>().HasIndex(x => new { x.TenantId, x.RunNo }).IsUnique();
        modelBuilder.Entity<Payslip>().HasIndex(x => new { x.TenantId, x.PayslipNo }).IsUnique();
        modelBuilder.Entity<PayrollPayment>().HasIndex(x => new { x.TenantId, x.PaymentNo }).IsUnique();
    }
}

public abstract class PayrollEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString("N");
}

public sealed class PayrollPolicy : PayrollEntity
{
    public string PolicyCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string CountryCode { get; set; } = "VN";
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public decimal SocialInsuranceEmployeeRate { get; set; } = 8;
    public decimal HealthInsuranceEmployeeRate { get; set; } = 1.5m;
    public decimal UnemploymentInsuranceEmployeeRate { get; set; } = 1;
    public decimal SocialInsuranceEmployerRate { get; set; } = 17.5m;
    public decimal HealthInsuranceEmployerRate { get; set; } = 3;
    public decimal UnemploymentInsuranceEmployerRate { get; set; } = 1;
    public decimal UnionFeeRate { get; set; } = 2;
    public decimal PersonalDeductionAmount { get; set; } = 11000000;
    public decimal DependentDeductionAmount { get; set; } = 4400000;
    public string Status { get; set; } = "Active";
}

public sealed class SalaryComponent : PayrollEntity
{
    public string ComponentCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string ComponentType { get; set; } = "Allowance";
    public bool Taxable { get; set; } = true;
    public bool InsuranceIncluded { get; set; }
    public bool Recurring { get; set; } = true;
    public string Formula { get; set; } = "";
    public int DisplayOrder { get; set; }
    public string Status { get; set; } = "Active";
}

public sealed class PayrollPeriod : PayrollEntity
{
    public string PeriodCode { get; set; } = "";
    public int Month { get; set; }
    public int Year { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateOnly PaymentDate { get; set; }
    public string Status { get; set; } = "Open";
    public DateTimeOffset? LockedAt { get; set; }
    public Guid? LockedBy { get; set; }
}

public sealed class PayrollRun : PayrollEntity
{
    public string RunNo { get; set; } = "";
    public Guid PeriodId { get; set; }
    public Guid? PayrollGroupId { get; set; }
    public string Status { get; set; } = "Draft";
    public decimal TotalGross { get; set; }
    public decimal TotalInsuranceEmployee { get; set; }
    public decimal TotalTaxableIncome { get; set; }
    public decimal TotalPit { get; set; }
    public decimal TotalNetPay { get; set; }
    public DateTimeOffset? CalculatedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
}

public sealed class PayrollLine : PayrollEntity
{
    public Guid PayrollRunId { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? PositionId { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal WorkingDays { get; set; }
    public decimal PaidLeaveDays { get; set; }
    public decimal UnpaidLeaveDays { get; set; }
    public decimal AbsentDays { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal GrossIncome { get; set; }
    public decimal InsuranceSalary { get; set; }
    public decimal EmployeeInsuranceAmount { get; set; }
    public decimal EmployerInsuranceAmount { get; set; }
    public decimal TaxableIncome { get; set; }
    public decimal PersonalDeduction { get; set; } = 11000000;
    public decimal DependentDeduction { get; set; }
    public decimal PitAmount { get; set; }
    public decimal TotalAllowance { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal NetPay { get; set; }
    public string PaymentStatus { get; set; } = "Unpaid";
}

public sealed class PayrollLineComponent : PayrollEntity
{
    public Guid PayrollLineId { get; set; }
    public Guid ComponentId { get; set; }
    public decimal Amount { get; set; }
    public string FormulaResult { get; set; } = "";
    public bool Taxable { get; set; }
    public bool InsuranceIncluded { get; set; }
}

public sealed class Payslip : PayrollEntity
{
    public string PayslipNo { get; set; } = "";
    public Guid PayrollLineId { get; set; }
    public Guid EmployeeId { get; set; }
    public string Status { get; set; } = "Draft";
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset? ViewedAt { get; set; }
    public Guid? FileId { get; set; }
}

public sealed class PayrollPayment : PayrollEntity
{
    public string PaymentNo { get; set; } = "";
    public Guid PayrollRunId { get; set; }
    public Guid EmployeeId { get; set; }
    public string PaymentMethod { get; set; } = "BankTransfer";
    public string BankAccountNo { get; set; } = "";
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTimeOffset? PaidAt { get; set; }
    public string ReferenceNo { get; set; } = "";
    public string FailureReason { get; set; } = "";
}
