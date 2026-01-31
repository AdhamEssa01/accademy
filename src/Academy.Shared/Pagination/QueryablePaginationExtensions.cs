using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Academy.Shared.Pagination;

public static class QueryablePaginationExtensions
{
    private const int MaxPageSize = 200;

    public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be at least 1.");
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), $"PageSize must be between 1 and {MaxPageSize}.");
        }

        if (query.Provider is IAsyncQueryProvider)
        {
            var totalAsync = await query.CountAsync(ct);
            var itemsAsync = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResponse<T>(itemsAsync, page, pageSize, totalAsync);
        }

        var total = query.Count();
        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResponse<T>(items, page, pageSize, total);
    }
}
