namespace Transaction.Aggregator.Domain.Models;

public record TransactionQuery
{
    public string AccountId { get; init; } = string.Empty;
    public string? SourceName { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
