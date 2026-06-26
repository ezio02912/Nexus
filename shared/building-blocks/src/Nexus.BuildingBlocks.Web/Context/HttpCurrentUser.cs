using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Nexus.SharedKernel.Context;

namespace Nexus.BuildingBlocks.Web.Context;

public sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public HttpCurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public Guid? Id
    {
        get
        {
            var raw = Principal?.FindFirstValue(NexusClaimTypes.UserId)
                ?? Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    public string? UserName =>
        Principal?.FindFirstValue(NexusClaimTypes.UserName) ?? Principal?.Identity?.Name;

    public IReadOnlyCollection<string> Permissions =>
        Principal?.FindAll(NexusClaimTypes.Permission).Select(x => x.Value).ToArray() ?? [];

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
}
