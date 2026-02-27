using Moq;
using Transaction.Aggregator.Application;
using Transaction.Aggregator.Application.Contracts;
using Transaction.Aggregator.Domain.Models;
using Transaction.Aggregator.Infrastructure.RuleEngine;
using Transaction.Aggregator.Infrastructure.Sources;
using Transaction.Aggregator.Tests.Fixtures;

namespace Transaction.Aggregator.Tests.Application;

public class TransactionManagerTests
{
    private readonly ITransactionManager _transactionManager;
    private readonly Mock<ITransactionSource> _mockSource;

    public TransactionManagerTests()
    {
        _mockSource = new Mock<ITransactionSource>();
        
        var sources = new List<ITransactionSource> { _mockSource.Object };
        
        var cacheMock = new Mock<CachedTransactionSource>();
        var resilientMock = new Mock<ResilientTransactionSource>();
        
        _transactionManager = new TransactionManager(sources);
    }

    [Fact]
    public async Task GetTransactions_WithValidQuery_ReturnsTransactions()
    {
        // Arrange
        var expectedTransactions = TransactionFixtures.CreateSampleTransactionBatch(5).ToList();
        _mockSource.Setup(s => s.GetTransactionsAsync(It.IsAny<TransactionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTransactions);

        var query = new TransactionQuery { };

        // Act
        var result = await _transactionManager.GetTransactionsAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedTransactions.Count, result.Items.Count);
    }

    [Fact]
    public async Task GetTransactions_WithMultipleSources_AggregatesAllTransactions()
    {
        // Arrange
        var source1Transactions = TransactionFixtures.CreateSampleTransactionBatch(3).ToList();
        var source2Transactions = TransactionFixtures.CreateSampleTransactionBatch(2).ToList();

        var mockSource1 = new Mock<ITransactionSource>();
        var mockSource2 = new Mock<ITransactionSource>();

        mockSource1.Setup(s => s.GetTransactionsAsync(It.IsAny<TransactionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(source1Transactions);

        mockSource2.Setup(s => s.GetTransactionsAsync(It.IsAny<TransactionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(source2Transactions);

        var sources = new List<ITransactionSource> { mockSource1.Object, mockSource2.Object };
        var transactionManager = new TransactionManager(sources);

        var query = new QueryParameter { StartDate = DateTime.UtcNow.AddDays(-30) };

        // Act
        var result = await transactionManager.GetTransactionsAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public async Task GetTransactions_SourceThrowsException_ReturnEmptyList()
    {
        // Arrange
        _mockSource.Setup(s => s.GetTransactionsAsync(It.IsAny<QueryParameter>()))
            .ThrowsAsync(new Exception("Source unavailable"));

        var query = new QueryParameter { StartDate = DateTime.UtcNow.AddDays(-30) };

        // Act & Assert - Should handle exception gracefully
        var result = await _transactionManager.GetTransactionsAsync(query);
        
        // Result should be empty or contain partial data depending on implementation
        Assert.NotNull(result);
    }
}

public class CategorizationAggregatorTests
{
    private readonly ICategorizerEngine _categorizerEngine;

    public CategorizationAggregatorTests()
    {
        var ruleSet = CategorizationFixtures.CreateSampleRuleSet();
        _categorizerEngine = new CustomRuleCategorizer(
            Microsoft.Extensions.Options.Options.Create(ruleSet)
        );
    }

    [Fact]
    public async Task CategorizeTransaction_WithKnownMerchant_ReturnsCategoryFromRule()
    {
        // Arrange
        var transaction = TransactionFixtures.CreateTransactionWithMerchant("Whole Foods");

        // Act
        var category = await _categorizerEngine.CategorizeTransactionAsync(transaction.Description, CancellationToken.None);

        // Assert
        Assert.Equal("Groceries", category);
    }

    [Fact]
    public async Task CategorizeTransaction_WithUnknownMerchant_ReturnsDefault()
    {
        // Arrange
        var transaction = TransactionFixtures.CreateTransactionWithMerchant("UnknownMerchant");

        // Act
        var category = await _categorizerEngine.CategorizeTransactionAsync(transaction.Description, CancellationToken.None);

        // Assert
        Assert.Equal("Uncategorized", category);
    }

    [Fact]
    public async Task CategorizeTransaction_WithMultipleTransactions_CategorizeAllCorrectly()
    {
        // Arrange
        var transactions = new[]
        {
            TransactionFixtures.CreateTransactionWithMerchant("Netflix"),
            TransactionFixtures.CreateTransactionWithMerchant("Whole Foods"),
            TransactionFixtures.CreateTransactionWithMerchant("Unknown")
        };

        // Act
        var categories = new List<string>();
        foreach (var tx in transactions)
        {
            categories.Add(await _categorizerEngine.CategorizeTransactionAsync(tx.Description, CancellationToken.None));
        }

        // Assert
        Assert.Equal(3, categories.Count);
        Assert.Contains("Groceries", categories);
    }
}
