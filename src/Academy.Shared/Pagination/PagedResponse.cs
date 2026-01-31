namespace Academy.Shared.Pagination;

public sealed class PagedResponse<T>
{
    public PagedResponse(IReadOnlyList<T> items, int page, int pageSize, long total)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        Total = total;
    }

    public IReadOnlyList<T> Items { get; }

    public int Page { get; }

    public int PageSize { get; }

    public long Total { get; }
}