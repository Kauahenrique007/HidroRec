namespace HidroRec.Backend.Application.DTOs.Common;

public sealed class PagedResultDto<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = Array.Empty<T>();

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalItems { get; init; }

    public int TotalPages { get; init; }
}
