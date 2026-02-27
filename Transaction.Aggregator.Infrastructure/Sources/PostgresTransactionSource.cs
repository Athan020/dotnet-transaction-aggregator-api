using System;
using Npgsql;
using Transaction.Aggregator.Application.Contracts;
using Transaction.Aggregator.Domain.Models;

namespace Transaction.Aggregator.Infrastructure;

public class PostgresTransactionSource : ITransactionSource
{
    private readonly string _connectionString;

    public string SourceName => "PostgreSQL";

    public PostgresTransactionSource(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async ValueTask<Result<TransactionItem[]>> GetTransactionsAsync(TransactionQuery query, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var sql = @"
                SELECT 
                    t.id,
                    t.description,
                    t.amount,
                    t.transaction_timestamp,
                    a.account_number,
                    c.name as category
                FROM transactions.transactions t
                JOIN transactions.accounts a ON t.account_id = a.id
                LEFT JOIN transactions.categories c ON t.category_id = c.id
                WHERE 1=1";

            if (query.AccountId > 0)
            {
                sql += " AND a.id = @accountId";
            }

            sql += " ORDER BY t.transaction_timestamp DESC";

            using var command = new NpgsqlCommand(sql, connection);
            
            if (query.AccountId > 0)
            {
                command.Parameters.AddWithValue("@accountId", query.AccountId);
            }

            // no LIMIT/OFFSET here – pagination is applied by TransactionManager

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var transactions = new List<TransactionItem>();

            while (await reader.ReadAsync(cancellationToken))
            {
                transactions.Add(new TransactionItem
                {
                    Id = reader.GetGuid(0).ToString(),
                    Description = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Amount = reader.GetDecimal(2),
                    Date = reader.GetDateTime(3),
                    FromAccountId = long.Parse(reader.GetString(4)),
                    Category = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Source = SourceName
                });
            }

            return Result<TransactionItem[]>.Success(transactions.ToArray());
        }
        catch (Exception ex)
        {
            return Result<TransactionItem[]>.Failure(new ErrorDetail{ Message = $"Failed to retrieve transactions from PostgreSQL: {ex.Message}"});
        }
    }
}
