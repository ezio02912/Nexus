namespace Nexus.Services.Identity.Domain.Users;

public static class UserErrorCodes
{
    public const string AlreadyExists = "Identity:UserAlreadyExists";
    public const string InvalidCredentials = "Identity:InvalidCredentials";
    public const string TenantRequired = "Identity:TenantRequired";
    public const string NotFound = "Identity:UserNotFound";
}
