using System;
using Transaction.Aggregator.Application.Contracts;
using Transaction.Aggregator.Domain.Models;

namespace Transaction.Aggregator.Tests.Fixtures;

public class FakeTransactionSource(string name, IReadOnlyList<TransactionItem> transactions, Exception? exception = null) : ITransactionSource
{
    public string SourceName => name;

    public ValueTask<Result<TransactionItem[]>> GetTransactionsAsync(TransactionQuery query, CancellationToken cancellationToken)
    {
        if (exception != null)
        {
            throw new Exception($"Error fetching transactions from {SourceName} source", exception);
        }

        return new ValueTask<Result<TransactionItem[]>>(Result<TransactionItem[]>.Success(transactions.ToArray()));
    }

    public static FakeTransactionSource Create(string name, IReadOnlyList<TransactionItem> transactions, Exception? exception = null)
    {
        return new FakeTransactionSource(name, transactions, exception);
    }

    public static FakeTransactionSource Create(string name, params TransactionItem[] transactions)
    {
        return new FakeTransactionSource(name, transactions);
    }

    public static FakeTransactionSource Create(string name, Exception exception)
    {
        return new FakeTransactionSource(name, [], exception);
    }
    
}
