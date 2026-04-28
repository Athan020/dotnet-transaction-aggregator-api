using System;
using Transaction.Aggregator.Application.Contracts;
using Transaction.Aggregator.Domain.Models;

namespace Transaction.Aggregator.Infrastructure;

public sealed class PrepaidTransactionSource : ITransactionSource
{
    public string SourceName => "Prepaid";

    public async ValueTask<Result<TransactionItem[]>> GetTransactionsAsync(TransactionQuery query, CancellationToken cancellationToken)
    {


        var transactions = new[]
        {
            new TransactionItem
            {
                Id = Guid.NewGuid().ToString(),
                FromAccountId = query.AccountId,
                Amount = -75.00m,
                Date = DateTime.UtcNow.AddDays(-3),
                Description = "Daily Lotto *Ithuba",
                Source = SourceName
            },
            new TransactionItem
            {
                Id = Guid.NewGuid().ToString(),
                FromAccountId = query.AccountId,
                Amount = -120.00m, 
                Date = DateTime.UtcNow.AddDays(-1),
                Description = "Vodacom *Topup",
                Source = SourceName,
            },
            new TransactionItem
            {
                Id = Guid.NewGuid().ToString(),
                Amount = 5000.00m, 
                Date = DateTime.UtcNow.AddDays(-4),
                Description = "Betway Withdrawls *Online",
                Source = SourceName
            }
        };

        return Result<TransactionItem[]>.Success(transactions);
    }
}
