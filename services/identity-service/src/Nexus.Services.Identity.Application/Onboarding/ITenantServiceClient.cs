using System.Globalization;
using System.Text;
using Nexus.Services.Tenant.Contracts.Tenants;

namespace Nexus.Services.Identity.Application.Onboarding;

public interface ITenantServiceClient
{
    Task<bool> IsCodeAvailableAsync(string code, CancellationToken cancellationToken = default);
    Task<TenantDto> CreateTenantAsync(CreateInternalTenantDto input, CancellationToken cancellationToken = default);
    Task<TenantDto?> GetTenantByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public static class TenantCodeGenerator
{
    public static string BuildSuggestedCode(string companyName)
    {
        var slug = Slugify(companyName);
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = "TENANT";
        }

        if (slug.Length > 8)
        {
            slug = slug[..8];
        }

        var suffix = Random.Shared.Next(1000, 9999);
        return $"{slug}-{suffix}";
    }

    public static string Slugify(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToUpperInvariant(character));
            }
        }

        return builder.ToString();
    }
}
