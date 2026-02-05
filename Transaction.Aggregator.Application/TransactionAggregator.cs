using System;
using Microsoft.Extensions.Logging;
using Transaction.Aggregator.Application;
using Transaction.Aggregator.Application.Contracts;
using Transaction.Aggregator.Domain.Models;

namespace Transaction.Aggregator.Application;

public sealed class TransactionAggregator(IEnumerable<ITransactionSource> transactionSources, IResiliencePipelineFactory resiliencePipelineFactory, ILogger<TransactionAggregator> logger) : ITransactionAggregator
{
    private readonly IEnumerable<ITransactionSource> _transactionSources = transactionSources;
    private readonly IResiliencePipelineFactory _resiliencePipelineFactory = resiliencePipelineFactory;
    private readonly ILogger<TransactionAggregator> _logger = logger;
    public async Task<IReadOnlyList<TransactionItem>?> AggregateTransactionsAsync(TransactionQuery query, CancellationToken cancellationToken)
    {

        var transactionSourcesTasks = _transactionSources
            .Where(source => query.SourceName == null || source.SourceName.Equals(query.SourceName, StringComparison.OrdinalIgnoreCase))
            .Select(source => CollectTransactionFromSourceSafelyAsync(query, source, cancellationToken))
            .ToArray();

        var results = await Task.WhenAll(transactionSourcesTasks);

        return [.. results
                    .SelectMany(tx => tx)
                    .DistinctBy(t => new { t.Id, t.Source })];
    }

    private async Task<TransactionItem[]> CollectTransactionFromSourceSafelyAsync(TransactionQuery query, ITransactionSource source, CancellationToken cancellationToken)
    {
        try
        {
            var pipeline = _resiliencePipelineFactory.GetOrCreatePipeline(source.SourceName);

            _logger.LogInformation("Collecting transactions from source: {SourceName}", source.SourceName);

            var result = await pipeline.ExecuteAsync(ctn => source.GetTransactionsAsync(query, ctn), cancellationToken);

            if (result.IsSuccess)
            {
                return result.Value!;
            }

            _logger.LogWarning("Failed to collect transactions from source: {SourceName} with reason {reason}", source.SourceName, result.Error?.Message);

            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while collecting transactions from source: {SourceName}", source.SourceName);
            return [];
        }
    }
}
