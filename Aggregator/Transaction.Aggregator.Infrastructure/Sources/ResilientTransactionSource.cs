using System;
using Transaction.Aggregator.Application;
using Transaction.Aggregator.Application.Contracts;
using Transaction.Aggregator.Domain.Models;

namespace Transaction.Aggregator.Infrastructure.Sources;

public class ResilientTransactionSource(ITransactionSource transactionSource, IResiliencePipelineFactory resiliencePipelineFactory) : ITransactionSource
{
    private ITransactionSource _transactionSource = transactionSource;
    private IResiliencePipelineFactory _pipelineFactory = resiliencePipelineFactory;

    public string SourceName => _transactionSource.SourceName;

    public async ValueTask<Result<TransactionItem[]>> GetTransactionsAsync(TransactionQuery query, CancellationToken cancellationToken)
    {
        var pipeline = _pipelineFactory.GetOrCreatePipeline(SourceName);

        return await pipeline.ExecuteAsync(ctn => _transactionSource.GetTransactionsAsync(query, ctn), cancellationToken);
    }
}
