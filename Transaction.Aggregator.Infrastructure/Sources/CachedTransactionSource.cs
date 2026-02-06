using System;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Transaction.Aggregator.Application.Contracts;
using Transaction.Aggregator.Domain.Models;

namespace Transaction.Aggregator.Infrastructure.Sources;

public sealed class CachedTransactionSource(ITransactionSource source, HybridCache hybridCache, ILogger<CachedTransactionSource> logger) : ITransactionSource
{

    private readonly HybridCache _cache = hybridCache;

    private readonly ILogger<CachedTransactionSource> _logger = logger;

    private readonly ITransactionSource _transactionSource = source;
    public string SourceName => _transactionSource.SourceName;

    public async ValueTask<Result<TransactionItem[]>> GetTransactionsAsync(TransactionQuery query, CancellationToken cancellationToken)
    {

        _logger.LogInformation("Attempting to retrieve transactions for {SourceName} from Cache", SourceName);

        var cacheKey = $"{SourceName}:{query.AccountId}";

        var entry = await _cache.GetOrCreateAsync(cacheKey, async (ct) =>
        { 
            _logger.LogInformation("Cache Miss for {SourceName} with CacheKey : {CacheKey}", SourceName,cacheKey );

            var cachedEntry = await _transactionSource.GetTransactionsAsync(query, ct);

            return cachedEntry;
            
            }, cancellationToken: cancellationToken);

        return entry;
    }
}
