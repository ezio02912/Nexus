namespace Nexus.Services.MasterData.Api;

public static class MasterDataCategories
{
    public const string Industry = "Industry";
    public const string City = "City";
    public const string Source = "Source";
    public const string Unit = "Unit";
    public const string ProductType = "ProductType";

    public static readonly IReadOnlyList<(string Code, string Label)> All =
    [
        (Industry, "Ngành"),
        (City, "Thành phố"),
        (Source, "Nguồn"),
        (Unit, "Đơn vị tính"),
        (ProductType, "Loại hàng hoá")
    ];

    public static string GetLabel(string category) =>
        All.FirstOrDefault(x => x.Code == category).Label ?? category;

    public static bool IsValid(string? category) =>
        !string.IsNullOrWhiteSpace(category) && All.Any(x => x.Code == category);
}
