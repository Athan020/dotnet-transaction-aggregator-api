using Transaction.Aggregator.Infrastructure.RuleEngine;
using Transaction.Aggregator.Infrastructure.Sources;

namespace Transaction.Aggregator.Tests.Infrastructure;

public class TransactionSourceTests
{
    [Fact]
    public void CardTransactionSource_HasCorrectName()
    {
        // Arrange
        var source = new CardTransactionSource();

        // Act
        var name = source.GetSourceName();

        // Assert
        Assert.Equal("Card", name);
    }

    [Fact]
    public void PrepaidTransactionSource_HasCorrectName()
    {
        // Arrange
        var source = new PrepaidTransactionSource();

        // Act
        var name = source.GetSourceName();

        // Assert
        Assert.Equal("Prepaid", name);
    }

    [Fact]
    public void RewardTransactionSource_HasCorrectName()
    {
        // Arrange
        var source = new RewardTransactionSource();

        // Act
        var name = source.GetSourceName();

        // Assert
        Assert.Equal("Reward", name);
    }
}

public class CategorizationRuleEngineTests
{
    [Fact]
    public void Rule_CanBeParsed_FromJson()
    {
        // This would integrate with your rule JSON
        var ruleSet = CategorizationFixtures.CreateSampleRuleSet();

        Assert.NotNull(ruleSet);
        Assert.NotEmpty(ruleSet.Rules);
    }

    [Fact]
    public void CustomRuleCategorizer_PicksExpectedCategory()
    {
        var ruleSet = CategorizationFixtures.CreateSampleRuleSet();
        var categorizerEngine = new CustomRuleCategorizer(
            Microsoft.Extensions.Options.Options.Create(ruleSet)
        );

        var category = categorizerEngine.CategorizeTransactionAsync("Buy groceries at supermarket", CancellationToken.None).Result;
        Assert.Equal("Groceries", category);
    }

    [Fact]
    public async Task DatabaseCategorizer_UsesRuleRepository()
    {
        // arrange fake repository with two rules
        var repo = new FakeRuleRepository();
        var engine = new DatabaseCategorizer(repo);

        // act
        var cat1 = await engine.CategorizeTransactionAsync("i need coffee", CancellationToken.None);
        var cat2 = await engine.CategorizeTransactionAsync("pay utility bill", CancellationToken.None);

        // assert
        Assert.Equal("Coffee", cat1);
        Assert.Equal("Utilities", cat2);
    }
}

// simple in-memory fake repo used by DatabaseCategorizer tests
internal class FakeRuleRepository : ICategorizationRuleRepository
{
    public Task<IEnumerable<CategorizationRule>> GetRulesAsync(CancellationToken cancellationToken = default)
    {
        var rules = new[]
        {
            new CategorizationRule
            {
                RuleName = "coffee",
                Category = "Coffee",
                DescriptionContains = new[] {"coffee"},
                Priority = 10
            },
            new CategorizationRule
            {
                RuleName = "utilities",
                Category = "Utilities",
                DescriptionContains = new[] {"utility"},
                Priority = 20
            }
        };
        return Task.FromResult((IEnumerable<CategorizationRule>)rules);
    }
}