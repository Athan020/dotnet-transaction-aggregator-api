using System;

namespace Transaction.Aggregator.Application.Contracts;

public interface ICategorizerEngine
{
    public ValueTask<string> CategorizeTransactionAsync(string description, CancellationToken cancellationToken);
}
