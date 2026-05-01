using System;
using Microsoft.Extensions.Logging;
using Transaction.Aggregator.Application.Contracts;
using Transaction.Aggregator.Domain.Models;

namespace Transaction.Aggregator.Application;

public class TransactionManager(ITransactionSource transactionAggregator, ILogger<TransactionManager> logger) : ITransactionManager
{
    private readonly ITransactionSource _transactionAggregator = transactionAggregator;
    private readonly ILogger<TransactionManager> _logger = logger;

    public async Task<PaginatedResult<TransactionItem>> GetTransactionsAsync(TransactionQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting transaction aggregation for AccountId: {AccountId}, PageNumber: {PageNumber}, PageSize: {PageSize}", query.AccountId, query.PageNumber, query.PageSize);

        var aggregatedTransactions = await _transactionAggregator.GetTransactionsAsync(query, cancellationToken);

        if(!aggregatedTransactions.IsSuccess)
        {
            _logger.LogError("Failed to aggregate transactions for AccountId: {AccountId}. Error: {ErrorMessage}", query.AccountId, aggregatedTransactions.Error);
            return new PaginatedResult<TransactionItem>
            {
                Items = [],
                TotalCount = 0,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }
        return new PaginatedResult<TransactionItem>
        {
            Items = aggregatedTransactions.Value!,
            TotalCount = aggregatedTransactions.Metadata.GetValueOrDefault("TotalCount", 0)  is int totalCount ? totalCount : 0,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }
}
