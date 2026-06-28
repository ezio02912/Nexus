using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;

namespace Nexus.Services.Hrm.Api.Persistence;

public sealed class HrmDbContext(DbContextOptions<HrmDbContext> options) : NexusDbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<EmployeeContract> EmployeeContracts => Set<EmployeeContract>();
    public DbSet<EmployeeHistory> EmployeeHistories => Set<EmployeeHistory>();
    public DbSet<EmployeeAllowance> EmployeeAllowances => Set<EmployeeAllowance>();
    public DbSet<EmployeeBenefit> EmployeeBenefits => Set<EmployeeBenefit>();
    public DbSet<EmployeeDocument> EmployeeDocuments => Set<EmployeeDocument>();
    public DbSet<JobRequisition> JobRequisitions => Set<JobRequisition>();
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<JobApplication> Applications => Set<JobApplication>();
    public DbSet<Interview> Interviews => Set<Interview>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<OnboardingChecklist> OnboardingChecklists => Set<OnboardingChecklist>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>().HasIndex(x => new { x.TenantId, x.EmployeeCode }).IsUnique();
        modelBuilder.Entity<Department>().HasIndex(x => new { x.TenantId, x.DepartmentCode }).IsUnique();
        modelBuilder.Entity<Position>().HasIndex(x => new { x.TenantId, x.PositionCode }).IsUnique();
        modelBuilder.Entity<EmployeeContract>().HasIndex(x => new { x.TenantId, x.ContractNo }).IsUnique();
        modelBuilder.Entity<EmployeeAllowance>().HasIndex(x => new { x.TenantId, x.EmployeeId, x.Name, x.EffectiveFrom });
        modelBuilder.Entity<EmployeeBenefit>().HasIndex(x => new { x.TenantId, x.EmployeeId, x.Name, x.StartDate });
        modelBuilder.Entity<EmployeeDocument>().HasIndex(x => new { x.TenantId, x.EmployeeId, x.DocumentType });
        modelBuilder.Entity<JobRequisition>().HasIndex(x => new { x.TenantId, x.RequisitionNo }).IsUnique();
        modelBuilder.Entity<Candidate>().HasIndex(x => new { x.TenantId, x.CandidateCode }).IsUnique();
        modelBuilder.Entity<Offer>().HasIndex(x => new { x.TenantId, x.OfferNo }).IsUnique();
    }
}

public abstract class HrmRecord
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString("N");
}

public sealed class Employee : HrmRecord
{
    public string EmployeeCode { get; set; } = "";
    public string FullName { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Gender { get; set; } = "";
    public DateOnly? DateOfBirth { get; set; }
    public string Nationality { get; set; } = "VN";
    public string MaritalStatus { get; set; } = "";
    public string PersonalEmail { get; set; } = "";
    public string WorkEmail { get; set; } = "";
    public string Phone { get; set; } = "";
    public string EmergencyContactName { get; set; } = "";
    public string EmergencyContactPhone { get; set; } = "";
    public string IdentityNo { get; set; } = "";
    public DateOnly? IdentityIssuedDate { get; set; }
    public string IdentityIssuedPlace { get; set; } = "";
    public string TaxCode { get; set; } = "";
    public string SocialInsuranceNo { get; set; } = "";
    public string PermanentAddress { get; set; } = "";
    public string CurrentAddress { get; set; } = "";
    public string BankName { get; set; } = "";
    public string BankAccountNo { get; set; } = "";
    public string BankAccountName { get; set; } = "";
    public Guid? DepartmentId { get; set; }
    public Guid? PositionId { get; set; }
    public Guid? ManagerId { get; set; }
    public string JobLevel { get; set; } = "";
    public string JobGrade { get; set; } = "";
    public string EmploymentStatus { get; set; } = "Draft";
    public string EmploymentType { get; set; } = "FullTime";
    public DateOnly? JoinDate { get; set; }
    public DateOnly? ProbationStartDate { get; set; }
    public DateOnly? ProbationEndDate { get; set; }
    public DateOnly? OfficialDate { get; set; }
    public DateOnly? ResignDate { get; set; }
    public string ResignReason { get; set; } = "";
    public decimal ProbationSalary { get; set; }
    public decimal OfficialSalary { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal PerformanceBonusPercent { get; set; }
    public string SalaryCurrency { get; set; } = "VND";
    public Guid? PayrollGroupId { get; set; }
    public Guid? WorkCalendarId { get; set; }
    public Guid? AvatarFileId { get; set; }
    public Guid? OwnerId { get; set; }
    public string Notes { get; set; } = "";
}

public sealed class EmployeeAllowance : HrmRecord
{
    public Guid EmployeeId { get; set; }
    public string AllowanceType { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public bool Taxable { get; set; } = true;
    public bool InsuranceIncluded { get; set; }
    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string Status { get; set; } = "Active";
}

public sealed class EmployeeBenefit : HrmRecord
{
    public Guid EmployeeId { get; set; }
    public string BenefitType { get; set; } = "";
    public string Name { get; set; } = "";
    public string PolicyCode { get; set; } = "";
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string Status { get; set; } = "Active";
    public string Notes { get; set; } = "";
}

public sealed class EmployeeDocument : HrmRecord
{
    public Guid EmployeeId { get; set; }
    public string DocumentType { get; set; } = "";
    public Guid FileId { get; set; }
    public string FileName { get; set; } = "";
    public DateOnly? IssuedDate { get; set; }
    public DateOnly? ExpiredDate { get; set; }
    public string Status { get; set; } = "Active";
    public string Notes { get; set; } = "";
}

public sealed class Department : HrmRecord
{
    public string DepartmentCode { get; set; } = "";
    public string Name { get; set; } = "";
    public Guid? ParentDepartmentId { get; set; }
    public Guid? ManagerId { get; set; }
    public string CostCenterCode { get; set; } = "";
    public string Location { get; set; } = "";
    public string Status { get; set; } = "Active";
    public string Description { get; set; } = "";
}

public sealed class Position : HrmRecord
{
    public string PositionCode { get; set; } = "";
    public string Name { get; set; } = "";
    public Guid? DepartmentId { get; set; }
    public string Level { get; set; } = "";
    public string JobGrade { get; set; } = "";
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }
    public string Description { get; set; } = "";
    public string Status { get; set; } = "Active";
}

public sealed class EmployeeContract : HrmRecord
{
    public string ContractNo { get; set; } = "";
    public Guid EmployeeId { get; set; }
    public string ContractType { get; set; } = "";
    public string Status { get; set; } = "Draft";
    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public DateOnly? SignedDate { get; set; }
    public Guid? SignedBy { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal AllowanceAmount { get; set; }
    public string Currency { get; set; } = "VND";
    public string WorkingLocation { get; set; } = "";
    public string ProbationTerms { get; set; } = "";
    public string TerminationTerms { get; set; } = "";
    public Guid? FileId { get; set; }
}

public sealed class EmployeeHistory : HrmRecord
{
    public Guid EmployeeId { get; set; }
    public string ChangeType { get; set; } = "";
    public DateOnly EffectiveDate { get; set; }
    public string OldValue { get; set; } = "";
    public string NewValue { get; set; } = "";
    public string Reason { get; set; } = "";
    public Guid? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
}

public sealed class JobRequisition : HrmRecord
{
    public string RequisitionNo { get; set; } = "";
    public string Title { get; set; } = "";
    public Guid? DepartmentId { get; set; }
    public Guid? PositionId { get; set; }
    public Guid? HiringManagerId { get; set; }
    public Guid? RecruiterId { get; set; }
    public int Headcount { get; set; }
    public string EmploymentType { get; set; } = "FullTime";
    public string WorkLocation { get; set; } = "";
    public decimal SalaryMin { get; set; }
    public decimal SalaryMax { get; set; }
    public string Currency { get; set; } = "VND";
    public string Reason { get; set; } = "";
    public string Priority { get; set; } = "Normal";
    public string Status { get; set; } = "Open";
    public DateOnly? OpenedDate { get; set; }
    public DateOnly? TargetStartDate { get; set; }
    public DateOnly? ClosedDate { get; set; }
}

public sealed class Candidate : HrmRecord
{
    public string CandidateCode { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Source { get; set; } = "";
    public string CurrentCompany { get; set; } = "";
    public string CurrentTitle { get; set; } = "";
    public decimal ExpectedSalary { get; set; }
    public string Currency { get; set; } = "VND";
    public int NoticePeriodDays { get; set; }
    public Guid? ResumeFileId { get; set; }
    public string PortfolioUrl { get; set; } = "";
    public string Status { get; set; } = "New";
    public string Tags { get; set; } = "";
}

public sealed class JobApplication : HrmRecord
{
    public Guid JobRequisitionId { get; set; }
    public Guid CandidateId { get; set; }
    public string Stage { get; set; } = "Screening";
    public string Status { get; set; } = "Active";
    public DateOnly AppliedDate { get; set; }
    public decimal ScreeningScore { get; set; }
    public decimal InterviewScore { get; set; }
    public decimal OfferSalary { get; set; }
    public string RejectReason { get; set; } = "";
    public Guid? OwnerId { get; set; }
}

public sealed class Interview : HrmRecord
{
    public Guid ApplicationId { get; set; }
    public int Round { get; set; }
    public string InterviewType { get; set; } = "";
    public DateTimeOffset ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
    public string Interviewers { get; set; } = "";
    public string LocationOrLink { get; set; } = "";
    public string Result { get; set; } = "Pending";
    public decimal Score { get; set; }
    public string Feedback { get; set; } = "";
}

public sealed class Offer : HrmRecord
{
    public Guid ApplicationId { get; set; }
    public string OfferNo { get; set; } = "";
    public string Status { get; set; } = "Draft";
    public decimal OfferedSalary { get; set; }
    public string Currency { get; set; } = "VND";
    public DateOnly? StartDate { get; set; }
    public Guid? OfferFileId { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }
    public string RejectReason { get; set; } = "";
}

public sealed class OnboardingChecklist : HrmRecord
{
    public Guid EmployeeId { get; set; }
    public Guid? OfferId { get; set; }
    public string ChecklistNo { get; set; } = "";
    public string Status { get; set; } = "Open";
    public string ItemsJson { get; set; } = "[]";
}
