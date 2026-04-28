using System;
using Transaction.Aggregator.Application.Contracts;
using Transaction.Aggregator.Domain.Models;

namespace Transaction.Aggregator.Infrastructure;

public class CardTransactionSource : ITransactionSource
{
    public string SourceName => "Card";

    public async ValueTask<Result<TransactionItem[]>> GetTransactionsAsync(TransactionQuery query, CancellationToken cancellationToken)
    {

        var transactions = new[]
        {
            new TransactionItem
            {
                Id = Guid.NewGuid().ToString(),
                Amount = -550.75m,
                FromAccountId = query.AccountId,
                Date = DateTime.UtcNow.AddDays(-1),
                Description = "APO CARD *Repayment - Thank you",
                Source = SourceName
            },
            new TransactionItem
            {
                Id = Guid.NewGuid().ToString(),
                Amount = -200.00m,
                FromAccountId = query.AccountId,
                Date = DateTime.UtcNow.AddDays(-5),
                Description = "Online Subscription - Netflix",
                Source = SourceName
            }, 
            new TransactionItem
            {
                Id = Guid.NewGuid().ToString(),
                Amount = -200.00m,
                FromAccountId = query.AccountId,
                Date = DateTime.UtcNow.AddDays(-10),
                Description = "Grocery Store",
                Source = SourceName
            },
             new TransactionItem
            {
                Id = Guid.NewGuid().ToString(),
                Amount = 35000.00m,
                Date = DateTime.UtcNow.AddDays(-15),
                Description = "Salary Deposit",
                Source = SourceName
            }
        };
        return Result<TransactionItem[]>.Success(transactions);
    }
}
