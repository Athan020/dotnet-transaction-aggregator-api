using System;
using Microsoft.Extensions.Options;
using Transaction.Aggregator.Application.Contracts;
using Transaction.Aggregator.Domain.Models;

namespace Transaction.Aggregator.Infrastructure.RuleEngine;

public sealed class CustomRuleCategorizer(IOptions<CategorizationRuleSet> options) : ICategorizerEngine
{
    private readonly CategorizationRuleSet _options = options.Value;

    public ValueTask<string> CategorizeTransactionAsync(string description, CancellationToken cancellationToken)
    {
        if(_options.IsEnabled == false)
        {
            return ValueTask.FromResult(_options.DefaultCategory);
        }

        foreach (var rule in _options.Rules.OrderBy(r => r.Priority))
        {
            if (rule.DescriptionContains.Any(keyword => description.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            {
                return ValueTask.FromResult(rule.Category ?? _options.DefaultCategory);
            }
        }

        return ValueTask.FromResult(_options.DefaultCategory);
    }
}
