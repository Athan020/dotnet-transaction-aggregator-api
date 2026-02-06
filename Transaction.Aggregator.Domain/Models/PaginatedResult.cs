using System;

namespace Transaction.Aggregator.Domain.Models;

public record class PaginatedResult<TResponse>
    where TResponse : class
{

    public IReadOnlyList<TResponse> Items { get; init; } = [];

    public int TotalCount { get; init; }

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
