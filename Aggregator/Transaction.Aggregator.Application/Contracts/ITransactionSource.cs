using System;
using Transaction.Aggregator.Domain.Models;

namespace Transaction.Aggregator.Application.Contracts;

public interface ITransactionSource
{
    string SourceName { get; }

    ValueTask<Result<PaginatedResult<TransactionItem>>> GetTransactionsAsync(TransactionQuery query, CancellationToken cancellationToken);
}
