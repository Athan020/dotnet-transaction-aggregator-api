using System;
using Microsoft.Extensions.Logging;
using Transaction.Aggregator.Application.Contracts;
using Transaction.Aggregator.Domain.Models;

namespace Transaction.Aggregator.Application;

public class TransactionManager(ITransactionAggregator transactionAggregator, ILogger<TransactionManager> logger) : ITransactionManager
{
    private readonly ITransactionAggregator _transactionAggregator = transactionAggregator;
    private readonly ILogger<TransactionManager> _logger = logger;

    public async Task<PaginatedResult<TransactionItem>> GetTransactionsAsync(TransactionQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting transaction aggregation for AccountId: {AccountId}, PageNumber: {PageNumber}, PageSize: {PageSize}", query.AccountId, query.PageNumber, query.PageSize);

        var aggregatedTransactions = await _transactionAggregator.AggregateTransactionsAsync(query, cancellationToken);

        var result = aggregatedTransactions?
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToArray() ?? [];

        _logger.LogInformation("Completed transaction aggregation for AccountId: {AccountId}. Total Transactions: {TotalCount}, Returned Transactions: {ReturnedCount}", query.AccountId, aggregatedTransactions?.Count ?? 0, result.Length);
        
        return new PaginatedResult<TransactionItem>
        {
            Items = result,
            TotalCount = aggregatedTransactions?.Count ?? 0,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }
}
