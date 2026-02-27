using Transaction.Aggregator.Domain.Models;

namespace Transaction.Aggregator.Tests.Fixtures;

public class TransactionFixtures
{
    private static readonly Random Random = new Random();

    public static Transaction CreateSampleTransaction(string? accountId = null, string? sourceId = null)
    {
        return new Transaction
        {
            Id = Guid.NewGuid().ToString(),
            AccountId = accountId ?? Guid.NewGuid().ToString(),
            SourceId = sourceId ?? Guid.NewGuid().ToString(),
            Description = "Sample Transaction",
            Amount = 150.00m,
            Currency = "USD",
            TransactionType = "debit",
            Merchant = "Test Merchant",
            Location = "New York",
            ReferenceNumber = Guid.NewGuid().ToString()[..12],
            TransactionTimestamp = DateTime.UtcNow,
            Category = "Shopping"
        };
    }

    public static IEnumerable<Transaction> CreateSampleTransactionBatch(int count = 10)
    {
        var batch = new List<Transaction>();
        for (int i = 0; i < count; i++)
        {
            batch.Add(CreateSampleTransaction());
        }
        return batch;
    }

    public static Transaction CreateTransactionWithAmount(decimal amount)
    {
        var transaction = CreateSampleTransaction();
        return transaction with { Amount = amount };
    }

    public static Transaction CreateTransactionWithCategory(string category)
    {
        var transaction = CreateSampleTransaction();
        return transaction with { Category = category };
    }

    public static Transaction CreateTransactionWithMerchant(string merchant)
    {
        var transaction = CreateSampleTransaction();
        return transaction with { Merchant = merchant };
    }
}

public class CategorizationFixtures
{
    public static CategorizationRuleSet CreateSampleRuleSet()
    {
        return new CategorizationRuleSet
        {
            Rules = new List<CategorizationRule>
            {
                new()
                {
                    Id = "rule-1",
                    Category = "Groceries",
                    Merchants = new[] { "Whole Foods", "Trader Joe's", "Kroger" }
                },
                new()
                {
                    Id = "rule-2",
                    Category = "Entertainment",
                    Merchants = new[] { "Netflix", "Spotify", "Disney+" }
                },
                new()
                {
                    Id = "rule-3",
                    Category = "Utilities",
                    MerchantPatterns = new[] { "ELECTRICITY", "WATER", "GAS" }
                }
            }
        };
    }
}

public class SourceFixtures
{
    public static readonly List<string> SourceNames = new()
    {
        "Card",
        "Prepaid",
        "Reward"
    };

    public static ITransactionSource CreateMockSource(string sourceName = "TestSource")
    {
        var mock = new Moq.Mock<ITransactionSource>();
        mock.Setup(s => s.GetSourceName()).Returns(sourceName);
        mock.Setup(s => s.GetTransactionsAsync(It.IsAny<QueryParameter>()))
            .ReturnsAsync(TransactionFixtures.CreateSampleTransactionBatch().ToList());

        return mock.Object;
    }
}
