using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Transaction.Aggregator.Application.Contracts;
using Transaction.Aggregator.Domain.Models;

namespace Transaction.Aggregator.Infrastructure.RuleEngine;

public sealed class PostgresCategorizationRuleRepository : ICategorizationRuleRepository
{
    private readonly string _connectionString;

    public PostgresCategorizationRuleRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<IEnumerable<CategorizationRule>> GetRulesAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<CategorizationRule>();
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        const string sql = @"SELECT rule_name, category, description_contains, priority
                             FROM transactions.categorization_rules
                             WHERE enabled = true
                             ORDER BY priority";

        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new CategorizationRule
            {
                RuleName = reader.GetString(0),
                Category = reader.GetString(1),
                DescriptionContains = reader.GetFieldValue<string[]>(2),
                Priority = reader.GetInt32(3)
            });
        }

        return results;
    }
}