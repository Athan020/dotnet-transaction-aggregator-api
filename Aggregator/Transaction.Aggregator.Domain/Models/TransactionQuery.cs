namespace Transaction.Aggregator.Domain.Models;

public record TransactionQuery
{
    public long AccountId { get; init; }
    public string? SourceName { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    public DateTimeOffset FromDate { get; init; } = DateTimeOffset.UtcNow.AddMonths(-3);
    public DateTimeOffset ToDate { get; init; } = DateTimeOffset.UtcNow;
}
