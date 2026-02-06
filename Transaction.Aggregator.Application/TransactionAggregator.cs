using System;
using Microsoft.Extensions.Logging;
using Transaction.Aggregator.Application;
using Transaction.Aggregator.Application.Contracts;
using Transaction.Aggregator.Domain.Models;

namespace Transaction.Aggregator.Application;

public sealed class TransactionAggregator(IEnumerable<ITransactionSource> transactionSources, ILogger<TransactionAggregator> logger) : ITransactionAggregator
{
    private readonly IEnumerable<ITransactionSource> _transactionSources = transactionSources;
    private readonly ILogger<TransactionAggregator> _logger = logger;
    public async Task<IReadOnlyList<TransactionItem>?> AggregateTransactionsAsync(TransactionQuery query, CancellationToken cancellationToken)
    {

        var transactionSourcesTasks = _transactionSources
            .Where(source => query.SourceName == null || source.SourceName.Equals(query.SourceName, StringComparison.OrdinalIgnoreCase))
            .Select(source => source.GetTransactionsAsync(query,cancellationToken).AsTask())
            .ToArray();

        var results = await Task.WhenAll(transactionSourcesTasks);

        return [.. results
                    .SelectMany(tx => tx.Value!)
                    .DistinctBy(t => new { t.Id, t.Source })];
    }
}
