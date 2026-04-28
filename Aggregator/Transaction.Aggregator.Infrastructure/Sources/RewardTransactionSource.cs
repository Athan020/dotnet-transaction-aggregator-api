using System;
using Transaction.Aggregator.Application.Contracts;
using Transaction.Aggregator.Domain.Models;

namespace Transaction.Aggregator.Infrastructure;

public sealed class RewardTransactionSource : ITransactionSource
{
    public string SourceName => "Reward";

    public async ValueTask<Result<TransactionItem[]>> GetTransactionsAsync(TransactionQuery query, CancellationToken cancellationToken)
    {
        var transactions = new[]
        {
            new TransactionItem
            {
                Id = Guid.NewGuid().ToString(),
                Amount = 150.00m,
                Date = DateTime.UtcNow.AddDays(-2),
                Description = "Woolworths *Constantia",
                Source = SourceName
            },
            new TransactionItem
            {
                Id = Guid.NewGuid().ToString(),
                Amount = 200.00m, 
                Date = DateTime.UtcNow.AddDays(-1),
                Description = "Spar *Rosmead",
                Source = SourceName
            }
        };

        return Result<TransactionItem[]>.Success(transactions);
        
    }
}
