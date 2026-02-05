using System;
using Transaction.Aggregator.Domain.Models;

namespace Transaction.Aggregator.Application.Contracts;

public interface ITransactionManager
{

    Task<PaginatedResult<TransactionItem>> GetTransactionsAsync(TransactionQuery query, CancellationToken cancellationToken);
}
