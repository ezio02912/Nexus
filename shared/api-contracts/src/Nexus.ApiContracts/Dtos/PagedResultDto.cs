namespace Nexus.ApiContracts.Dtos;

public sealed class PagedResultDto<T>
{
    public long TotalCount { get; init; }
    public IReadOnlyList<T> Items { get; init; } = [];
}
