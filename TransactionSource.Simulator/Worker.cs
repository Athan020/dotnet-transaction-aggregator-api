using TransactionSource.Simulator.Services;
using Microsoft.Extensions.Logging;

namespace TransactionSource.Simulator;

public class SimulatorWorker(
    IKafkaProducerService kafkaProducer,
    ITransactionGenerator transactionGenerator,
    ILogger<SimulatorWorker> logger,
    IConfiguration configuration)
    : BackgroundService
{
    private readonly int _batchSize = configuration.GetValue<int>("SimulatorConfig:TransactionBatchSize", 10);
    private readonly int _batchIntervalSeconds = configuration.GetValue<int>("SimulatorConfig:BatchIntervalSeconds", 30);
    private readonly int _numberOfSources = configuration.GetValue<int>("SimulatorConfig:NumberOfSources", 3);

    private static readonly string[] SourceNames = { "card", "prepaid", "reward" };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Transaction Simulator starting up with configuration: BatchSize={BatchSize}, Interval={Interval}s, Sources={Sources}",
            _batchSize, _batchIntervalSeconds, _numberOfSources);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_batchIntervalSeconds));

        try
        {
            // Run immediately on startup
            await PublishTransactionBatches(stoppingToken);

            // Then run periodically
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await PublishTransactionBatches(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Simulator cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception in simulator");
            throw;
        }
    }

    private async Task PublishTransactionBatches(CancellationToken stoppingToken)
    {
        for (int i = 0; i < _numberOfSources; i++)
        {
            try
            {
                var sourceId = SourceNames[i % SourceNames.Length];
                logger.LogInformation("Publishing transaction batch for source {Source}", sourceId);

                var transactions = transactionGenerator.GenerateTransactionBatch(sourceId, _batchSize);

                foreach (var transaction in transactions)
                {
                    await kafkaProducer.PublishTransactionAsync(transaction);
                    if (stoppingToken.IsCancellationRequested)
                        return;
                }

                logger.LogInformation("Successfully published {Count} transactions for source {Source}", _batchSize, sourceId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error publishing transactions for source {Source}", SourceNames[i % SourceNames.Length]);
            }
        }

        // Flush any remaining messages
        try
        {
            await kafkaProducer.FlushAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error flushing Kafka producer");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Transaction Simulator stopping");
        await base.StopAsync(cancellationToken);
    }
}

