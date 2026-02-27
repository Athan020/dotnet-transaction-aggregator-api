using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace TransactionSource.Simulator.Services;

public interface IKafkaProducerService
{
    Task PublishTransactionAsync(object transaction);
    Task FlushAsync();
}

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;
    private readonly ILogger<KafkaProducerService> _logger;

    public KafkaProducerService(string bootstrapServers, string topic, ILogger<KafkaProducerService> logger)
    {
        _topic = topic;
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            AllowAutoCreateTopics = true,
            Acks = Acks.All,
            MessageTimeoutMs = 30000,
            RequestTimeoutMs = 30000,
        };

        _producer = new ProducerBuilder<string, string>(config)
            .SetErrorHandler((_, e) =>
            {
                _logger.LogError($"Kafka error: {e.Reason}");
            })
            .SetLogHandler((_, m) =>
            {
                if (m.Level >= SyslogLevel.Warning)
                {
                    _logger.LogWarning($"Kafka: {m.Message}");
                }
            })
            .Build();
    }

    public async Task PublishTransactionAsync(object transaction)
    {
        try
        {
            var json = JsonSerializer.Serialize(transaction);
            var key = Guid.NewGuid().ToString();

            var deliveryReport = await _producer.ProduceAsync(_topic, new Message<string, string>
            {
                Key = key,
                Value = json
            });

            _logger.LogInformation(
                $"Transaction published to {deliveryReport.Topic} partition {deliveryReport.Partition} at offset {deliveryReport.Offset}");
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError($"Failed to publish transaction: {ex.Error.Reason}");
            throw;
        }
    }

    public async Task FlushAsync()
    {
        _producer.Flush(TimeSpan.FromSeconds(30));
    }
}
