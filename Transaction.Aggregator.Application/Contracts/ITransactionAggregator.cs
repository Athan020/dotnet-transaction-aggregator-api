using System;
using Transaction.Aggregator.Domain.Models;

namespace Transaction.Aggregator.Application.Contracts;

public interface ITransactionAggregator
{
    Task<IReadOnlyList<TransactionItem>?> AggregateTransactionsAsync(TransactionQuery query, CancellationToken cancellationToken);
}
