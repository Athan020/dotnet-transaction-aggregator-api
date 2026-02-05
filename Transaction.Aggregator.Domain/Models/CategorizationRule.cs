using System;

namespace Transaction.Aggregator.Domain.Models;

public class CategorizationRule
{
    public string RuleName { get; set; } = string.Empty;

    public string? Category { get; set; } = string.Empty;

    public string[] DescriptionContains { get; set; } = [];

    public int Priority { get; set; }
}

public class CategorizationRuleSet
{
    public bool IsEnabled { get; set; }

    public string DefaultCategory { get; set; } = "Uncategorized";
    
    public List<CategorizationRule> Rules { get; set; } = [];
}
