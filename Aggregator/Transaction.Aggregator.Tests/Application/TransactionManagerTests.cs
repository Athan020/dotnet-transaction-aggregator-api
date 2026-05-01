using System;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Transaction.Aggregator.Application;
using Transaction.Aggregator.Application.Contracts;
using Transaction.Aggregator.Domain.Models;
using Transaction.Aggregator.Tests.Fixtures;

namespace Transaction.Aggregator.Tests.Application;

public class TransactionManagerTests
{
    [Fact]
    public async Task Should_GetTransactionsAsync_Return_Aggregated_Transactions()
    {
        TransactionItem[] transactions =
        [
            TransactionFixtures.Salary(),
            TransactionFixtures.Grocery(),
            TransactionFixtures.OnlineSubscription()
        ];

        Mock<ITransactionSource> transactionAggregatorMock = new();
        transactionAggregatorMock
            .Setup(a => a.GetTransactionsAsync(It.IsAny<TransactionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<TransactionItem[]>.Success(transactions));

        var transactionManager = new TransactionManager(transactionAggregatorMock.Object, NullLogger<TransactionManager>.Instance);

        var query = new TransactionQuery { PageNumber = 1, PageSize = 10 };
        var result = await transactionManager.GetTransactionsAsync(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(query.PageNumber, result.PageNumber);
        Assert.Equal(query.PageSize, result.PageSize);
        
        transactionAggregatorMock.Verify(a => a.GetTransactionsAsync(It.IsAny<TransactionQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
