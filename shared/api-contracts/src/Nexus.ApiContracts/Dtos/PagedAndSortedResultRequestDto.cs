namespace Nexus.ApiContracts.Dtos;

public class PagedAndSortedResultRequestDto
{
    public int SkipCount { get; init; }
    public int MaxResultCount { get; init; } = 50;
    public string? Sorting { get; init; }
}
