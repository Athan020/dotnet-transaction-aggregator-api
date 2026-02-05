using System;

namespace Transaction.Aggregator.Domain.Models;

public record class PaginatedResult<TResponse>
    where TResponse : class
{

    public IReadOnlyList<TResponse> Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
