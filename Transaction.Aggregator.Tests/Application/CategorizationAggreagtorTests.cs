using System;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Transaction.Aggregator.Application;
using Transaction.Aggregator.Application.Contracts;
using Transaction.Aggregator.Domain.Models;
using Transaction.Aggregator.Tests.Fixtures;

namespace Transaction.Aggregator.Tests.Application;

public class CategorizationAggreagtorTests
{

    [Fact]
    public async Task Should_AggregateTransactionsAsync_And_Then_Categorize_Them()
    {
        var transactions = new List<TransactionItem>
        {
            TransactionFixtures.Salary(),
            TransactionFixtures.Grocery(),
            TransactionFixtures.OnlineSubscription()
        };

        var transactionAggregatorMock = new Mock<ITransactionAggregator>();
        transactionAggregatorMock.Setup(a => a.AggregateTransactionsAsync(It.IsAny<TransactionQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(transactions);

        var categorizerEngineMock = new Mock<ICategorizerEngine>();
        categorizerEngineMock.Setup(e => e.CategorizeTransactionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((string description, CancellationToken ct) =>
        {    return description switch
            {
                "Monthly Salary" => "Income",
                "Grocery Store" => "Groceries",
                "Online Subscription" => "Subscriptions",
                _ => "Uncategorized"
            };
        });

        var categorizationAggregator = new CategorizationAggregator(transactionAggregatorMock.Object, categorizerEngineMock.Object, NullLogger<CategorizationAggregator>.Instance);

        var result = await categorizationAggregator.AggregateTransactionsAsync(new TransactionQuery(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(3, result?.Count);
        Assert.All(result!, transaction =>
        {
            Assert.NotNull(transaction.Category);
        });
        Assert.Equal("Income", result?[0].Category);
        Assert.Equal("Groceries", result?[1].Category);
    }

}
