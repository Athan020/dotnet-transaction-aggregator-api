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
        PaginatedResult<TransactionItem> paginatedResult = new();

        var aggregatedTransactions = await _transactionAggregator.AggregateTransactionsAsync(query, cancellationToken);

        paginatedResult.Items = aggregatedTransactions?
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToArray() ?? [];
        
        paginatedResult.TotalCount = aggregatedTransactions?.Count ?? 0;
        paginatedResult.PageNumber = query.PageNumber;
        paginatedResult.PageSize = query.PageSize;

        return paginatedResult;
    }
}
