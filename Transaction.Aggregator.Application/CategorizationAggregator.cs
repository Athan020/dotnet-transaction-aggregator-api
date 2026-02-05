using System;
using Microsoft.Extensions.Logging;
using Transaction.Aggregator.Application.Contracts;
using Transaction.Aggregator.Domain.Models;

namespace Transaction.Aggregator.Application;

public sealed class CategorizationAggregator(ITransactionAggregator transactionAggregator, ICategorizerEngine categorizerEngine, ILogger<CategorizationAggregator> logger) : ITransactionAggregator
{
    private readonly ITransactionAggregator _transactionAggregator = transactionAggregator;

    private readonly ICategorizerEngine _categorizerEngine = categorizerEngine;

    private readonly ILogger<CategorizationAggregator> _logger = logger;

    public async Task<IReadOnlyList<TransactionItem>?> AggregateTransactionsAsync(TransactionQuery query, CancellationToken cancellationToken)
    {


        var transactions = await _transactionAggregator.AggregateTransactionsAsync(query, cancellationToken);

        foreach (var transaction in transactions ?? [])
        {
            transaction.Category = await _categorizerEngine.CategorizeTransactionAsync(transaction.Description, cancellationToken) ?? "Uncategorized";
        }

        return transactions;
    }
}
