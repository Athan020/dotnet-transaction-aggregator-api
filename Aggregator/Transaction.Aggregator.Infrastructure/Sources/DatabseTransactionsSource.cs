using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Entities;
using Transaction.Aggregator.Application.Contracts;
using Transaction.Aggregator.Domain.Models;

namespace Transaction.Aggregator.Infrastructure.Sources;

public sealed class DatabaseTransactionsSource(TransactionsContext transactionsContext, ILogger<DatabaseTransactionsSource> logger) : ITransactionSource
{
    public string SourceName => nameof(DatabaseTransactionsSource);

    private readonly TransactionsContext _transactionsContext = transactionsContext;
    private readonly ILogger<DatabaseTransactionsSource> _logger = logger;
    public async ValueTask<Result<TransactionItem[]>> GetTransactionsAsync(TransactionQuery query, CancellationToken cancellationToken)
    {

        _logger.LogInformation("Retrieving transactions for {SourceName} from Database for AccountId : {AccountId}", SourceName ?? "All Sources", query.AccountId);

        var transactions =  _transactionsContext.Transactions
            .Where(trx => trx.AccountId == query.AccountId && trx.TransactionDate >= query.FromDate && trx.TransactionDate <= query.ToDate);

        if(query.SourceName is not null)
        {
            transactions = transactions.Where(trx => trx.Source!.Name == query.SourceName);
        }

        var totalCount = await transactions.CountAsync(cancellationToken);
            
        var transactionItems = await transactions
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(trx => new TransactionItem
            {
                Id = trx.Id.ToString(),
                Amount = trx.Amount,
                FromAccountId = trx.AccountId,
                Date = trx.TransactionDate,
                Description = trx.Description!,
                Category = trx.Category!.Name,
                SubCategory = trx.Category.SubcategoryOfNavigation != null ? trx.Category.SubcategoryOfNavigation.Name : string.Empty,
                Source = trx.Source!.Name,
                Currency = trx.Currency
            }).ToArrayAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} transactions for {SourceName} from Database for AccountId : {AccountId}", transactionItems.Length, SourceName ?? "All Sources", query.AccountId);

        return Result<TransactionItem[]>.Success(transactionItems, new Dictionary<string, object>
        {
            { "TotalCount", totalCount }
        });
    }
}
