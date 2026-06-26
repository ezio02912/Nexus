using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Nexus.BuildingBlocks.Web.Auth;
using Nexus.BuildingBlocks.Web.Context;
using Nexus.SharedKernel.Authorization;
using Nexus.SharedKernel.Context;

namespace Nexus.BuildingBlocks.Web.DependencyInjection;

public static class NexusWebExtensions
{
    /// <summary>
    /// Registers HttpContext-backed ambient accessors (user / tenant / correlation) and the
    /// claim-based permission infrastructure.
    /// </summary>
    public static IServiceCollection AddNexusWeb(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddScoped<ICurrentTenant, HttpCurrentTenant>();
        services.AddScoped<ICorrelationContext, HttpCorrelationContext>();
        services.AddScoped<IPermissionChecker, ClaimBasedPermissionChecker>();

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Configures JWT bearer authentication using the "Jwt" configuration section.
    /// </summary>
    public static IServiceCollection AddNexusJwtAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(NexusJwtOptions.SectionName).Get<NexusJwtOptions>() ?? new NexusJwtOptions();
        services.Configure<NexusJwtOptions>(configuration.GetSection(NexusJwtOptions.SectionName));

        var keyBytes = Encoding.UTF8.GetBytes(string.IsNullOrWhiteSpace(options.SigningKey)
            ? "nexus-development-signing-key-please-override-0123456789"
            : options.SigningKey);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                jwt.MapInboundClaims = false;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = options.Issuer,
                    ValidateAudience = true,
                    ValidAudience = options.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    NameClaimType = NexusClaimTypes.UserName,
                    RoleClaimType = NexusClaimTypes.Role
                };
            });

        return services;
    }
}
