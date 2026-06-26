namespace Nexus.SharedKernel.Context;

public interface ICurrentUser
{
    Guid? Id { get; }
    string? UserName { get; }
    IReadOnlyCollection<string> Permissions { get; }
    bool IsAuthenticated { get; }
}
