using System;
using Categorization;
using Shared.Entities;
using static Categorization.Categorization;

namespace Transaction.Ingestions.Worker.Workers;

public sealed class CategorizationWorker(IServiceScopeFactory serviceScopeFactory, ILogger<CategorizationWorker> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<CategorizationWorker> _logger = logger;

    private readonly PeriodicTimer _timer = new(TimeSpan.FromMinutes(2));
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Categorization Worker started at: {time}", DateTimeOffset.Now);

        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var transactionContext = scope.ServiceProvider.GetRequiredService<TransactionsContext>();

                var uncategorizedTransactions = transactionContext.Transactions
                    .Where(t => t.CategoryId == null)
                    .Take(100)
                    .AsQueryable();


                if (!uncategorizedTransactions.Any())
                {
                    _logger.LogInformation("No uncategorized transactions found at: {time}", DateTimeOffset.Now);
                    continue;
                }

                _logger.LogInformation("Found {Count} uncategorized transactions. Starting categorization at: {time}", uncategorizedTransactions.Count(), DateTimeOffset.Now);

                var categorizationBatchRequest = new CategorizeTransactionsBatchRequest
                {
                    Requests = { uncategorizedTransactions.Select(t => new CategorizeTransactionRequest
                    {
                        TransactionId = t.Id.ToString(),
                        Description = t.Description ?? string.Empty,                
                    }) }
                };

                var categorizationClient = scope.ServiceProvider.GetRequiredService<CategorizationClient>();

                var categorizationBatchResponse = await categorizationClient.CategorizeTransactionsBatchAsync(categorizationBatchRequest, cancellationToken: stoppingToken);

                foreach (var response in categorizationBatchResponse.Responses)
                {
                    var transactionId = Guid.Parse(response.TransactionId);
                    var transaction = uncategorizedTransactions.FirstOrDefault(t => t.Id == transactionId);

                    transaction?.CategoryId = Guid.Parse(response.CategoryId);
                }
                
                await transactionContext.SaveChangesAsync(stoppingToken);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while categorizing transactions.");
            }
        }

        _logger.LogInformation("Categorization Worker is stopping at: {time}", DateTimeOffset.Now);
    }

}
