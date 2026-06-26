using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Nexus.SharedKernel.Context;

namespace Nexus.BuildingBlocks.Web.Context;

public sealed class HttpCurrentTenant : ICurrentTenant
{
    private readonly IHttpContextAccessor _accessor;

    public HttpCurrentTenant(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public Guid? Id
    {
        get
        {
            var context = _accessor.HttpContext;
            var raw = context?.User.FindFirstValue(NexusClaimTypes.TenantId);
            if (string.IsNullOrWhiteSpace(raw) && context is not null &&
                context.Request.Headers.TryGetValue("x-tenant-id", out var header))
            {
                raw = header.ToString();
            }

            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    public string? Name => null;

    public bool IsAvailable => Id.HasValue;
}
