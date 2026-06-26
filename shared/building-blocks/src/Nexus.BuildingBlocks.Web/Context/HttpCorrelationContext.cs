using Microsoft.AspNetCore.Http;
using Nexus.SharedKernel.Context;

namespace Nexus.BuildingBlocks.Web.Context;

public sealed class HttpCorrelationContext : ICorrelationContext
{
    public const string HeaderName = "x-correlation-id";

    private readonly IHttpContextAccessor _accessor;

    public HttpCorrelationContext(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public string? CorrelationId
    {
        get
        {
            var context = _accessor.HttpContext;
            if (context is null)
            {
                return null;
            }

            return context.Request.Headers.TryGetValue(HeaderName, out var value)
                ? value.ToString()
                : context.TraceIdentifier;
        }
    }
}
