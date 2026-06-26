namespace Nexus.BuildingBlocks.Web.Auth;

public sealed class NexusJwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "nexus";
    public string Audience { get; set; } = "nexus";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 14;
}
