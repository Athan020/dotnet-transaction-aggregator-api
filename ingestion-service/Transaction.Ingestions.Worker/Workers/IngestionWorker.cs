using System;
using Microsoft.EntityFrameworkCore;
using Shared.Entities;
using Transaction.Ingestions.Worker.SourceAdapters;


namespace Transaction.Ingestions.Worker.Workers;

public sealed class IngestionWorker(IServiceScopeFactory serviceScopeFactory, ILogger<IngestionWorker> logger) : BackgroundService
{

    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<IngestionWorker> _logger = logger;
    private readonly PeriodicTimer _timer = new(TimeSpan.FromMinutes(1));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Ingestion Worker started at: {time}", DateTimeOffset.Now);

        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var sourceAdapters = scope.ServiceProvider.GetServices<ISourceAdapter>();
                var transactionContext = scope.ServiceProvider.GetRequiredService<TransactionsContext>();

                if(sourceAdapters == null || !sourceAdapters.Any())
                {
                    _logger.LogWarning("No source adapters found. Skipping this cycle.");
                    continue;
                }

                foreach (var adapter in sourceAdapters)
                {
                    _logger.LogInformation("Fetching transactions from source: {SourceName}", adapter.SourceName);

                    var source = transactionContext.Sources.FirstOrDefault(s => s.Name == adapter.SourceName);

                    if (source == null)
                    {
                        _logger.LogWarning("Source {SourceName} not found in database. Skipping.", adapter.SourceName);
                        continue;
                    }

                    if (!source.LastSynced.HasValue)
                    {
                        source.LastSynced = DateTime.UtcNow.AddDays(-90); // Default to 90 day ago if never synced
                    }

                    if (source.LastSynced.Value > DateTime.UtcNow || source.LastSynced.Value < DateTime.UtcNow.AddYears(-1))
                    {
                        _logger.LogWarning("Source {SourceName} has an invalid LastSynced date. Resetting to 90 days ago.", adapter.SourceName);
                        source.LastSynced = DateTime.UtcNow.AddDays(-90);
                    }

                    List<Shared.Entities.Transaction> transactions = [];
                    await foreach (var transaction in adapter.FetchTransactionsAsync(source.LastSynced.Value, stoppingToken))
                    {
                        transaction.Source = source;
                        transaction.CategoryId = null;
                        transaction.Category = null;

                        transactions.Add(transaction);
                    }

                    await transactionContext.Transactions
                        .UpsertRange(transactions)
                        .On(t => new { t.SourceId, t.ExternalId })
                        .WhenMatched((existing, incoming) => new Shared.Entities.Transaction
                        {
                            CategoryId = existing.CategoryId,
                            Category = existing.Category,
                            UpdatedAt = DateTime.UtcNow
                        })
                        .RunAsync(stoppingToken);

                    await transactionContext.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching transactions.");
            }
        }

        _logger.LogInformation("Ingestion Worker is stopping at: {time}", DateTimeOffset.Now);
    }

}
