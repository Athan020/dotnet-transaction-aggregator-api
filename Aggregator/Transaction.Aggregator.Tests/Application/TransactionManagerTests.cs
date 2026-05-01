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

        var transactionAggregatorMock = new Mock<ITransactionSource>();
        transactionAggregatorMock
            .Setup(a => a.GetTransactionsAsync(It.IsAny<TransactionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaginatedResult<TransactionItem>>.Success(new PaginatedResult<TransactionItem>
            {
                Items = transactions,
                TotalCount = transactions.Length,
                PageNumber = 1,
                PageSize = 10
            }));

        var transactionManager = new TransactionManager(transactionAggregatorMock.Object, NullLogger<TransactionManager>.Instance);
        var query = new TransactionQuery { PageNumber = 1, PageSize = 10 };

        var result = await transactionManager.GetTransactionsAsync(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(1, result.TotalPages);

        transactionAggregatorMock.Verify(a => a.GetTransactionsAsync(It.IsAny<TransactionQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_Return_Empty_Result_When_Source_Returns_No_Transactions()
    {
        var transactionAggregatorMock = new Mock<ITransactionSource>();
        transactionAggregatorMock
            .Setup(a => a.GetTransactionsAsync(It.IsAny<TransactionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaginatedResult<TransactionItem>>.Success(new PaginatedResult<TransactionItem>
            {
                Items = [],
                TotalCount = 0,
                PageNumber = 2,
                PageSize = 5
            }));

        var transactionManager = new TransactionManager(transactionAggregatorMock.Object, NullLogger<TransactionManager>.Instance);
        var query = new TransactionQuery { PageNumber = 2, PageSize = 5 };

        var result = await transactionManager.GetTransactionsAsync(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public async Task Should_Return_Empty_Result_When_Source_Fails()
    {
        var transactionAggregatorMock = new Mock<ITransactionSource>();
        transactionAggregatorMock
            .Setup(a => a.GetTransactionsAsync(It.IsAny<TransactionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaginatedResult<TransactionItem>>.Failure(new ErrorDetail
            {
                Source = nameof(ITransactionSource),
                Message = "Database unavailable"
            }));

        var transactionManager = new TransactionManager(transactionAggregatorMock.Object, NullLogger<TransactionManager>.Instance);
        var query = new TransactionQuery { PageNumber = 1, PageSize = 20 };

        var result = await transactionManager.GetTransactionsAsync(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public async Task Should_Pass_Query_To_TransactionSource()
    {
        var expectedQuery = new TransactionQuery
        {
            AccountId = 42,
            PageNumber = 3,
            PageSize = 5,
            FromDate = DateTimeOffset.UtcNow.AddDays(-30),
            ToDate = DateTimeOffset.UtcNow
        };

        var transactionAggregatorMock = new Mock<ITransactionSource>();
        transactionAggregatorMock
            .Setup(a => a.GetTransactionsAsync(It.Is<TransactionQuery>(q =>
                q.AccountId == expectedQuery.AccountId &&
                q.PageNumber == expectedQuery.PageNumber &&
                q.PageSize == expectedQuery.PageSize &&
                q.FromDate == expectedQuery.FromDate &&
                q.ToDate == expectedQuery.ToDate
            ), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaginatedResult<TransactionItem>>.Success(new PaginatedResult<TransactionItem>
            {
                Items = [TransactionFixtures.Lotto()],
                TotalCount = 1,
                PageNumber = expectedQuery.PageNumber,
                PageSize = expectedQuery.PageSize
            }));

        var transactionManager = new TransactionManager(transactionAggregatorMock.Object, NullLogger<TransactionManager>.Instance);

        await transactionManager.GetTransactionsAsync(expectedQuery, CancellationToken.None);

        transactionAggregatorMock.Verify(a => a.GetTransactionsAsync(It.Is<TransactionQuery>(q =>
            q.AccountId == expectedQuery.AccountId &&
            q.PageNumber == expectedQuery.PageNumber &&
            q.PageSize == expectedQuery.PageSize &&
            q.FromDate == expectedQuery.FromDate &&
            q.ToDate == expectedQuery.ToDate
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_Calculate_TotalPages_Based_On_Result_Count_And_PageSize()
    {
        TransactionItem[] transactions =
        [
            TransactionFixtures.Salary(),
            TransactionFixtures.Grocery(),
            TransactionFixtures.OnlineSubscription()
        ];

        var transactionAggregatorMock = new Mock<ITransactionSource>();
        transactionAggregatorMock
            .Setup(a => a.GetTransactionsAsync(It.IsAny<TransactionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaginatedResult<TransactionItem>>.Success(new PaginatedResult<TransactionItem>
            {
                Items = transactions,
                TotalCount = transactions.Length,
                PageNumber = 1,
                PageSize = 2
            }));

        var transactionManager = new TransactionManager(transactionAggregatorMock.Object, NullLogger<TransactionManager>.Instance);
        var query = new TransactionQuery { PageNumber = 1, PageSize = 2 };

        var result = await transactionManager.GetTransactionsAsync(query, CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(2, result.TotalPages);
    }
}
