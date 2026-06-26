namespace Nexus.Services.Crm.Domain.Enums;

public enum CustomerType
{
    Company = 0,
    Individual = 1
}

public enum CustomerStatus
{
    Active = 0,
    Inactive = 1,
    Prospect = 2
}

public enum LeadRating
{
    Hot = 0,
    Warm = 1,
    Cold = 2
}

public enum LeadStatus
{
    New = 0,
    Contacted = 1,
    Qualified = 2,
    Unqualified = 3,
    Converted = 4,
    Lost = 5
}

public enum OpportunityStage
{
    Prospecting = 0,
    Qualification = 1,
    Proposal = 2,
    Negotiation = 3,
    ClosedWon = 4,
    ClosedLost = 5
}

public enum QuotationStatus
{
    Draft = 0,
    Sent = 1,
    Approved = 2,
    Rejected = 3,
    Expired = 4,
    Cancelled = 5
}

public enum ContractStatus
{
    Draft = 0,
    PendingSign = 1,
    Signed = 2,
    Active = 3,
    Expired = 4,
    Terminated = 5,
    Cancelled = 6
}

public enum CrmActivityType
{
    Call = 0,
    Email = 1,
    Meeting = 2,
    Task = 3,
    Note = 4
}

public enum CrmActivityStatus
{
    Planned = 0,
    Completed = 1,
    Cancelled = 2
}

public enum CrmRelatedEntityType
{
    Customer = 0,
    Lead = 1,
    Opportunity = 2,
    Quotation = 3,
    Contract = 4
}
