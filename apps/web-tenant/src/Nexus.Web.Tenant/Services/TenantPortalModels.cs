namespace Nexus.Web.Tenant.Services;

public sealed record PagedResult<T>(long TotalCount, IReadOnlyList<T> Items);
public sealed record TenantSubscriptionDto(string PlanCode, string PlanName, decimal MonthlyPrice, DateTimeOffset? ExpiresAt);
public sealed record TenantDto(
    Guid Id,
    string Code,
    string Name,
    string Status,
    IReadOnlyList<TenantModuleDto>? Modules,
    IReadOnlyDictionary<string, string>? Settings,
    string? ConcurrencyStamp,
    TenantSubscriptionDto? Subscription = null);
public sealed record SubscriptionPlanDto(string PlanCode, string Name, decimal MonthlyPrice, IReadOnlyList<string>? Modules, int MaxUsers, int StorageGb, int TierOrder);
public sealed record CreateCheckoutRequest(string TargetPlanCode);
public sealed record CheckoutSessionDto(Guid CheckoutId, string TargetPlanCode, string TargetPlanName, decimal Amount, string MockCardNumber);
public sealed record SubscriptionPaymentDto(Guid Id, Guid TenantId, string PlanCode, decimal Amount, string Status, string? MockReference, DateTimeOffset CreatedAt, DateTimeOffset? PaidAt);
public sealed record TenantModuleDto(string ModuleCode, bool IsEnabled);
public sealed record LoginRequest(Guid TenantId, string UserName, string Password);
public sealed record LoginEmailRequest(string Email, string Password);
public sealed record LoginResult(Guid UserId, Guid TenantId, string AccessToken, DateTimeOffset ExpiresAt, string? RefreshToken = null, string? TenantCode = null, string? UserName = null, string? Email = null);
public sealed record GoogleAuthRequest(string? IdToken, string? AccessToken = null);
public sealed record GoogleAuthResult(string Status, string? OnboardingToken, string? Email, string? DisplayName, LoginResult? Login);
public sealed record PreviewTenantCodeRequest(string CompanyName);
public sealed record PreviewTenantCodeResult(string SuggestedCode, bool Available);
public sealed record CompleteOnboardingRequest(
    string OnboardingToken,
    string CompanyName,
    string Code,
    string RepresentativeName,
    string? Address,
    string? Phone,
    string? UserName,
    string? Password);
public sealed record CompleteOnboardingResult(Guid TenantId, string TenantCode, string TenantName, LoginResult Login);
public sealed record UserDto(Guid Id, Guid TenantId, string UserName, string Email, bool IsActive, IReadOnlyCollection<string>? Roles, string? ConcurrencyStamp);
public sealed record CreateUserRequest(Guid TenantId, string UserName, string Email, string Password, IReadOnlyCollection<string> Roles);
public sealed record RolePermissionDto(string RoleName, IReadOnlyCollection<string> Permissions);
public sealed record UpdateRolePermissionsRequest(IReadOnlyCollection<string> Permissions);

public sealed record SalesOrderRecord(Guid Id, Guid TenantId, Guid CustomerId, string OrderNo, string? SourceType, Guid? SourceId, string? SourceNo, string Status, string InventoryReservationStatus, string DeliveryStatus, decimal Subtotal, decimal DiscountAmount, decimal TaxAmount, decimal TotalAmount, IReadOnlyList<SalesOrderLineRecord>? Lines, DateTimeOffset CreatedAt, DateTimeOffset? ApprovedAt, DateTimeOffset? ReservedAt, DateTimeOffset? DeliveredAt, DateTimeOffset? CompletedAt);
public sealed record SalesOrderLineRecord(Guid Id, string WarehouseCode, string ProductCode, string Description, decimal Quantity, decimal UnitPrice, decimal DiscountPercent, decimal DiscountAmount, decimal TaxPercent, decimal TaxAmount, decimal Subtotal, decimal LineAmount);
public sealed record CreateSalesOrderRequest(Guid TenantId, Guid CustomerId, string OrderNo, string? SourceType, Guid? SourceId, string? SourceNo, IReadOnlyCollection<CreateSalesOrderLineRequest> Lines);
public sealed record CreateSalesOrderLineRequest(string WarehouseCode, string ProductCode, string Description, decimal Quantity, decimal UnitPrice, decimal DiscountPercent, decimal TaxPercent);

// Read-models below use mutable properties so they can bind to BootstrapBlazor
// Table columns (@bind-Field). They are only deserialized from JSON, never built positionally.
public sealed record StockBalanceRecord
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string WarehouseCode { get; set; } = "";
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public decimal OnHandQuantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
public sealed record ImportStockRequest(Guid TenantId, string WarehouseCode, string ProductCode, string ProductName, decimal Quantity, string? SourceType, Guid? SourceId, string? SourceNo);
public sealed record TransferStockRequest(Guid TenantId, string FromWarehouseCode, string ToWarehouseCode, string ProductCode, string? ProductName, decimal Quantity, string? SourceNo);
public sealed record StockTransferRecord
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TransferNo { get; set; } = "";
    public string FromWarehouseCode { get; set; } = "";
    public string ToWarehouseCode { get; set; } = "";
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public decimal Quantity { get; set; }
    public string Status { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
}
public sealed record StockMovementRecord
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string WarehouseCode { get; set; } = "";
    public string ProductCode { get; set; } = "";
    public string MovementType { get; set; } = "";
    public decimal Quantity { get; set; }
    public string SourceType { get; set; } = "";
    public Guid SourceId { get; set; }
    public string SourceNo { get; set; } = "";
    public DateTimeOffset OccurredAt { get; set; }
}
public sealed record InventoryProductRecord
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string Unit { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
    public decimal TaxPercent { get; set; }
    public bool IsActive { get; set; }
    public string Attributes { get; set; } = "";
    public string Variants { get; set; } = "";
    public DateTimeOffset UpdatedAt { get; set; }
}
public sealed record UpsertInventoryProductRequest(Guid TenantId, string ProductCode, string ProductName, string Unit, string? Category, decimal Price, decimal TaxPercent, bool IsActive, string? Attributes, string? Variants);
public sealed record WarehouseRecord
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string WarehouseCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string Location { get; set; } = "";
    public bool IsActive { get; set; }
    public bool AllowNegativeStock { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
public sealed record UpsertWarehouseRequest(Guid TenantId, string WarehouseCode, string Name, string? Location, bool IsActive, bool AllowNegativeStock);

public sealed record SupplierRecord
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string SupplierCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
public sealed record UpsertSupplierRequest(Guid TenantId, string SupplierCode, string Name, string? Email, string? Phone);
public sealed record PurchaseOrderRecord
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string PurchaseOrderNo { get; set; } = "";
    public string SupplierCode { get; set; } = "";
    public string SupplierName { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public IReadOnlyList<PurchaseOrderLineRecord>? Lines { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? ReceivedAt { get; set; }
}
public sealed record PurchaseOrderLineRecord(Guid Id, string WarehouseCode, string ProductCode, string ProductName, decimal Quantity, decimal UnitCost, decimal LineAmount);
public sealed record CreatePurchaseOrderRequest(Guid TenantId, string PurchaseOrderNo, string SupplierCode, IReadOnlyCollection<CreatePurchaseOrderLineRequest> Lines);
public sealed record CreatePurchaseOrderLineRequest(string WarehouseCode, string ProductCode, string ProductName, decimal Quantity, decimal UnitCost);
public sealed record ReceivePurchaseOrderRequest(string? ReceiptNo);
public sealed record GoodsReceiptRecord
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public string PurchaseOrderNo { get; set; } = "";
    public string ReceiptNo { get; set; } = "";
    public IReadOnlyList<GoodsReceiptLineRecord>? Lines { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }
}
public sealed record GoodsReceiptLineRecord(Guid Id, string WarehouseCode, string ProductCode, string ProductName, decimal Quantity, decimal UnitCost);

public abstract record TenantEntityRecord
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string ConcurrencyStamp { get; set; } = "";
}

public sealed record HrmEmployeeRecord : TenantEntityRecord
{
    public string EmployeeCode { get; set; } = "";
    public string FullName { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Gender { get; set; } = "";
    public DateOnly? DateOfBirth { get; set; }
    public string Nationality { get; set; } = "VN";
    public string PersonalEmail { get; set; } = "";
    public string WorkEmail { get; set; } = "";
    public string Phone { get; set; } = "";
    public Guid? DepartmentId { get; set; }
    public Guid? PositionId { get; set; }
    public Guid? ManagerId { get; set; }
    public string JobLevel { get; set; } = "";
    public string JobGrade { get; set; } = "";
    public string EmploymentStatus { get; set; } = "";
    public string EmploymentType { get; set; } = "";
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
    public string Notes { get; set; } = "";
}

public sealed record HrmEmployeeAllowanceRecord : TenantEntityRecord
{
    public Guid EmployeeId { get; set; }
    public string AllowanceType { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public bool Taxable { get; set; }
    public bool InsuranceIncluded { get; set; }
    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string Status { get; set; } = "";
}

public sealed record HrmEmployeeBenefitRecord : TenantEntityRecord
{
    public Guid EmployeeId { get; set; }
    public string BenefitType { get; set; } = "";
    public string Name { get; set; } = "";
    public string PolicyCode { get; set; } = "";
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string Status { get; set; } = "";
    public string Notes { get; set; } = "";
}

public sealed record HrmEmployeeDocumentRecord : TenantEntityRecord
{
    public Guid EmployeeId { get; set; }
    public string DocumentType { get; set; } = "";
    public Guid FileId { get; set; }
    public string FileName { get; set; } = "";
    public DateOnly? IssuedDate { get; set; }
    public DateOnly? ExpiredDate { get; set; }
    public string Status { get; set; } = "";
    public string Notes { get; set; } = "";
}

public sealed record HrmDepartmentRecord : TenantEntityRecord
{
    public string DepartmentCode { get; set; } = "";
    public string Name { get; set; } = "";
    public Guid? ParentDepartmentId { get; set; }
    public Guid? ManagerId { get; set; }
    public string CostCenterCode { get; set; } = "";
    public string Location { get; set; } = "";
    public string Status { get; set; } = "";
    public string Description { get; set; } = "";
}

public sealed record HrmPositionRecord : TenantEntityRecord
{
    public string PositionCode { get; set; } = "";
    public string Name { get; set; } = "";
    public Guid? DepartmentId { get; set; }
    public string Level { get; set; } = "";
    public string JobGrade { get; set; } = "";
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }
    public string Description { get; set; } = "";
    public string Status { get; set; } = "";
}

public sealed record HrmContractRecord : TenantEntityRecord
{
    public string ContractNo { get; set; } = "";
    public Guid EmployeeId { get; set; }
    public string ContractType { get; set; } = "";
    public string Status { get; set; } = "";
    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public DateOnly? SignedDate { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal AllowanceAmount { get; set; }
    public string Currency { get; set; } = "VND";
    public string WorkingLocation { get; set; } = "";
}

public sealed record HrmCandidateRecord : TenantEntityRecord
{
    public string CandidateCode { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Source { get; set; } = "";
    public decimal ExpectedSalary { get; set; }
    public string Currency { get; set; } = "VND";
    public string Status { get; set; } = "";
    public string Tags { get; set; } = "";
}

public sealed record HrmRequisitionRecord : TenantEntityRecord
{
    public string RequisitionNo { get; set; } = "";
    public string Title { get; set; } = "";
    public Guid? DepartmentId { get; set; }
    public Guid? PositionId { get; set; }
    public int Headcount { get; set; }
    public string EmploymentType { get; set; } = "";
    public string WorkLocation { get; set; } = "";
    public decimal SalaryMin { get; set; }
    public decimal SalaryMax { get; set; }
    public string Status { get; set; } = "";
}

public sealed record HrmApplicationRecord : TenantEntityRecord
{
    public Guid JobRequisitionId { get; set; }
    public Guid CandidateId { get; set; }
    public string Stage { get; set; } = "";
    public string Status { get; set; } = "";
    public DateOnly AppliedDate { get; set; }
    public decimal ScreeningScore { get; set; }
    public decimal InterviewScore { get; set; }
    public decimal OfferSalary { get; set; }
}

public sealed record HrmOfferRecord : TenantEntityRecord
{
    public Guid ApplicationId { get; set; }
    public string OfferNo { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal OfferedSalary { get; set; }
    public string Currency { get; set; } = "VND";
    public DateOnly? StartDate { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
}

public sealed record AttendanceCalendarRecord : TenantEntityRecord
{
    public string CalendarCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string CountryCode { get; set; } = "VN";
    public string WorkingDays { get; set; } = "";
    public TimeOnly DefaultStartTime { get; set; }
    public TimeOnly DefaultEndTime { get; set; }
    public TimeOnly BreakStartTime { get; set; }
    public TimeOnly BreakEndTime { get; set; }
    public decimal StandardHoursPerDay { get; set; }
    public bool IsDefault { get; set; }
    public string Status { get; set; } = "";
}

public sealed record AttendanceShiftRecord : TenantEntityRecord
{
    public string ShiftCode { get; set; } = "";
    public string Name { get; set; } = "";
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public decimal StandardHours { get; set; }
    public string Status { get; set; } = "";
}

public sealed record AttendanceRecordItem : TenantEntityRecord
{
    public Guid EmployeeId { get; set; }
    public Guid? DepartmentId { get; set; }
    public DateOnly WorkDate { get; set; }
    public Guid? ShiftId { get; set; }
    public DateTimeOffset? CheckInAt { get; set; }
    public DateTimeOffset? CheckOutAt { get; set; }
    public int LateMinutes { get; set; }
    public int EarlyLeaveMinutes { get; set; }
    public int WorkedMinutes { get; set; }
    public int OvertimeMinutes { get; set; }
    public string Status { get; set; } = "";
    public string CorrectionStatus { get; set; } = "";
}

public sealed record LeaveRequestRecord : TenantEntityRecord
{
    public string RequestNo { get; set; } = "";
    public Guid EmployeeId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public decimal TotalDays { get; set; }
    public string Reason { get; set; } = "";
    public string Status { get; set; } = "";
    public Guid? ApproverId { get; set; }
}

public sealed record LeaveTypeRecord : TenantEntityRecord
{
    public string LeaveTypeCode { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsPaid { get; set; }
    public decimal AnnualQuotaDays { get; set; }
    public bool CarryForwardAllowed { get; set; }
    public decimal MaxCarryForwardDays { get; set; }
    public bool RequiresApproval { get; set; }
    public string Status { get; set; } = "";
}

public sealed record HolidayRecord : TenantEntityRecord
{
    public DateOnly HolidayDate { get; set; }
    public string Name { get; set; } = "";
    public string HolidayType { get; set; } = "";
    public bool IsPaid { get; set; }
    public string CountryCode { get; set; } = "VN";
    public int Year { get; set; }
    public string Source { get; set; } = "";
}

public sealed record OvertimeRequestRecord : TenantEntityRecord
{
    public string RequestNo { get; set; } = "";
    public Guid EmployeeId { get; set; }
    public DateOnly WorkDate { get; set; }
    public TimeOnly FromTime { get; set; }
    public TimeOnly ToTime { get; set; }
    public decimal TotalHours { get; set; }
    public string OvertimeType { get; set; } = "";
    public decimal RateMultiplier { get; set; }
    public string Status { get; set; } = "";
}

public sealed record PayrollPolicyRecord : TenantEntityRecord
{
    public string PolicyCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string CountryCode { get; set; } = "VN";
    public DateOnly EffectiveFrom { get; set; }
    public decimal SocialInsuranceEmployeeRate { get; set; }
    public decimal HealthInsuranceEmployeeRate { get; set; }
    public decimal UnemploymentInsuranceEmployeeRate { get; set; }
    public decimal PersonalDeductionAmount { get; set; }
    public decimal DependentDeductionAmount { get; set; }
    public string Status { get; set; } = "";
}

public sealed record SalaryComponentRecord : TenantEntityRecord
{
    public string ComponentCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string ComponentType { get; set; } = "";
    public bool Taxable { get; set; }
    public bool InsuranceIncluded { get; set; }
    public bool Recurring { get; set; }
    public string Formula { get; set; } = "";
    public string Status { get; set; } = "";
}

public sealed record PayrollPeriodRecord : TenantEntityRecord
{
    public string PeriodCode { get; set; } = "";
    public int Month { get; set; }
    public int Year { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateOnly PaymentDate { get; set; }
    public string Status { get; set; } = "";
}

public sealed record PayrollRunRecord : TenantEntityRecord
{
    public string RunNo { get; set; } = "";
    public Guid PeriodId { get; set; }
    public string Status { get; set; } = "";
    public decimal TotalGross { get; set; }
    public decimal TotalInsuranceEmployee { get; set; }
    public decimal TotalTaxableIncome { get; set; }
    public decimal TotalPit { get; set; }
    public decimal TotalNetPay { get; set; }
    public DateTimeOffset? CalculatedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
}

public sealed record PayslipRecord : TenantEntityRecord
{
    public string PayslipNo { get; set; } = "";
    public Guid PayrollLineId { get; set; }
    public Guid EmployeeId { get; set; }
    public string Status { get; set; } = "";
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset? ViewedAt { get; set; }
}

public sealed record PayrollPaymentRecord : TenantEntityRecord
{
    public string PaymentNo { get; set; } = "";
    public Guid PayrollRunId { get; set; }
    public Guid EmployeeId { get; set; }
    public string PaymentMethod { get; set; } = "";
    public decimal Amount { get; set; }
    public string Status { get; set; } = "";
    public DateTimeOffset? PaidAt { get; set; }
    public string ReferenceNo { get; set; } = "";
}

public sealed record TenantScopedRequest(Guid TenantId);
public sealed record ApprovalRequest(Guid? ApproverId);
