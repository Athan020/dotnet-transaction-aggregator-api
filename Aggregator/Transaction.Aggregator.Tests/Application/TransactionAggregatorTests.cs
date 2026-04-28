using System;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Transaction.Aggregator.Application;
using Transaction.Aggregator.Application.Contracts;
using Transaction.Aggregator.Domain.Models;
using Transaction.Aggregator.Infrastructure.Sources;
using Transaction.Aggregator.Tests.Fixtures;

namespace Transaction.Aggregator.Tests.Application;

public class TransactionAggregatorTests
{

    [Fact]
    public async Task Should_Return_Transactions_When_All_Sources_Return_Transactions()
    {
        // Arrange
        var source1 = FakeTransactionSource.Create("source1", [TransactionFixtures.Salary()]);
        var source2 = FakeTransactionSource.Create("source2", [TransactionFixtures.Grocery()]);
        var source3 = FakeTransactionSource.Create("source3", [TransactionFixtures.OnlineSubscription()]);

        var transactionAggregator = new TransactionAggregator(new[] { source1, source2, source3 }, NullLogger<TransactionAggregator>.Instance);

        // Act
        var transactions = await transactionAggregator.AggregateTransactionsAsync(new TransactionQuery(), CancellationToken.None);

        // Assert
        Assert.NotNull(transactions);
        Assert.Equal(3, transactions?.Count);
    }

    [Fact]
    public async Task Should_Return_Transactions_From_Successful_Sources_When_Some_Sources_Fail()
    {
        // Arrange
        var source1 = FakeTransactionSource.Create("source1", [TransactionFixtures.Salary()]);
        var source2 = FakeTransactionSource.Create("source2", new Exception("Source failure"));
        var source3 = FakeTransactionSource.Create("source3", [TransactionFixtures.OnlineSubscription()]);

        var transactionAggregator = new TransactionAggregator(new[] { source1, source2, source3 }, NullLogger<TransactionAggregator>.Instance);

        // Act
        var transactions = await transactionAggregator.AggregateTransactionsAsync(new TransactionQuery(), CancellationToken.None);

        // Assert
        Assert.NotNull(transactions);
        Assert.Equal(2, transactions?.Count);
        Assert.DoesNotContain(transactions!, t => t.Source == "source2");
    }

    [Fact]
    public async Task Should_Return_Empty_List_When_All_Sources_Fail()
    {
        // Arrange
        var source1 = FakeTransactionSource.Create("source1", new Exception("Source failure"));
        var source2 = FakeTransactionSource.Create("source2", new Exception("Source failure"));
        var source3 = FakeTransactionSource.Create("source3", new Exception("Source failure"));

        Mock<IResiliencePipelineFactory> mockResilienceFactory = new();

        mockResilienceFactory.Setup(m => m.GetOrCreatePipeline(It.IsAny<string>())).Returns(Polly.ResiliencePipeline.Empty);

        var resilientSource1 = new ResilientTransactionSource(source1, mockResilienceFactory.Object);
        var resilientSource2 = new ResilientTransactionSource(source2, mockResilienceFactory.Object);
        var resilientSource3 = new ResilientTransactionSource(source3, mockResilienceFactory.Object);

        var transactionAggregator = new TransactionAggregator(new[] { resilientSource1, resilientSource2, resilientSource3 }, NullLogger<TransactionAggregator>.Instance);

        // Act
        var transactions = await transactionAggregator.AggregateTransactionsAsync(new TransactionQuery(), CancellationToken.None);

        // Assert
        Assert.NotNull(transactions);
        Assert.Empty(transactions!);
    }

    [Fact]
    public async Task Should_Not_Include_Duplicate_Transactions_From_Same_Sources()
    {
        // Arrange
        var transaction = TransactionFixtures.Salary();
        var source1 = FakeTransactionSource.Create("source1", [transaction, transaction]);

        var transactionAggregator = new TransactionAggregator(new[] { source1 }, NullLogger<TransactionAggregator>.Instance);

        // Act
        var transactions = await transactionAggregator.AggregateTransactionsAsync(new TransactionQuery(), CancellationToken.None);

        // Assert
        Assert.NotNull(transactions);
        Assert.Single(transactions!);
    }

}