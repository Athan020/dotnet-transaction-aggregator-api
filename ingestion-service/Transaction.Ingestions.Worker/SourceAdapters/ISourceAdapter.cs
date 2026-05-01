using System;

namespace Transaction.Ingestions.Worker.SourceAdapters;

public interface ISourceAdapter
{

    public string  SourceName { get; }

    public IAsyncEnumerable<Shared.Entities.Transaction> FetchTransactionsAsync(DateTimeOffset startDate, CancellationToken cancellationToken);

}

public record TransactionRecord
{
    public string SourceId { get; init; } = null!;
    public decimal Amount { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public string Description { get; init; } = null!;
    public string Currency { get; init; } = null!;
    public Dictionary<string, object> Metadata { get; init; } = [];

}