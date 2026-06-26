namespace Nexus.Services.MasterData.Api;

public static class MasterDataCategories
{
    public const string Industry = "Industry";
    public const string City = "City";
    public const string Source = "Source";

    public static readonly IReadOnlyList<(string Code, string Label)> All =
    [
        (Industry, "Ngành"),
        (City, "Thành phố"),
        (Source, "Nguồn")
    ];

    public static string GetLabel(string category) =>
        All.FirstOrDefault(x => x.Code == category).Label ?? category;

    public static bool IsValid(string? category) =>
        !string.IsNullOrWhiteSpace(category) && All.Any(x => x.Code == category);
}
