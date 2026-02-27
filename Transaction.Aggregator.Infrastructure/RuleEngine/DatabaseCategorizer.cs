using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Transaction.Aggregator.Application.Contracts;
using Transaction.Aggregator.Domain.Models;

namespace Transaction.Aggregator.Infrastructure.RuleEngine;

public sealed class DatabaseCategorizer : ICategorizerEngine
{
    private readonly ICategorizationRuleRepository _repository;
    private List<CategorizationRule> _cache = new();
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public DatabaseCategorizer(ICategorizationRuleRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async ValueTask<string> CategorizeTransactionAsync(string description, CancellationToken cancellationToken)
    {
        if (_cache.Count == 0)
        {
            await RefreshCacheAsync(cancellationToken).ConfigureAwait(false);
        }

        foreach (var rule in _cache.OrderBy(r => r.Priority))
        {
            if (rule.DescriptionContains.Any(keyword =>
                    description.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            {
                return rule.Category ?? "Uncategorized";
            }
        }

        return "Uncategorized";
    }

    private async Task RefreshCacheAsync(CancellationToken cancellationToken)
    {
        await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var rules = await _repository.GetRulesAsync(cancellationToken).ConfigureAwait(false);
            _cache = rules.ToList();
        }
        finally
        {
            _refreshLock.Release();
        }
    }
}