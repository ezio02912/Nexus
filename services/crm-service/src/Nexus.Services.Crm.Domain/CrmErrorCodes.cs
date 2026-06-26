namespace Nexus.Services.Crm.Domain;

public static class CrmErrorCodes
{
    public const string CustomerNotFound = "Crm:CustomerNotFound";
    public const string CustomerAlreadyExists = "Crm:CustomerAlreadyExists";
    public const string ContactNotFound = "Crm:ContactNotFound";
    public const string LeadNotFound = "Crm:LeadNotFound";
    public const string LeadAlreadyConverted = "Crm:LeadAlreadyConverted";
    public const string OpportunityNotFound = "Crm:OpportunityNotFound";
    public const string QuotationNotFound = "Crm:QuotationNotFound";
    public const string QuotationAlreadyExists = "Crm:QuotationAlreadyExists";
    public const string ContractNotFound = "Crm:ContractNotFound";
    public const string ContractAlreadyExists = "Crm:ContractAlreadyExists";
    public const string ActivityNotFound = "Crm:ActivityNotFound";
    public const string InvalidStatusTransition = "Crm:InvalidStatusTransition";
}
