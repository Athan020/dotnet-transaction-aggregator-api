using TransactionSource.Simulator;
using TransactionSource.Simulator.Services;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.AddConsole();

// Configuration
var kafkaBrokers = builder.Configuration.GetValue<string>("Kafka:BootstrapServers") ?? "localhost:9092";
var kafkaTopic = builder.Configuration.GetValue<string>("Kafka:TopicName") ?? "transactions";
var jaegerEndpoint = builder.Configuration.GetValue<string>("OpenTelemetry:JaegerEndpoint")
    ?? Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")
    ?? "http://jaeger:14268/api/traces";

// Add OpenTelemetry
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService("TransactionSourceSimulator", serviceVersion: "1.0.0")
    .AddTelemetrySdk()
    .AddAttributes(new[] {
        new KeyValuePair<string, object>("environment", builder.Environment.EnvironmentName),
        new KeyValuePair<string, object>("service.namespace", "aggregator")
    });

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .SetResourceBuilder(resourceBuilder)
            .AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri(jaegerEndpoint);
            });
    });

builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(resourceBuilder)
        .AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Endpoint = new Uri(jaegerEndpoint);
        });

    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
});

// Add services
builder.Services.AddSingleton<IKafkaProducerService>(sp =>
    new KafkaProducerService(kafkaBrokers, kafkaTopic, sp.GetRequiredService<ILogger<KafkaProducerService>>())
);
builder.Services.AddSingleton<ITransactionGenerator, TransactionGenerator>();
builder.Services.AddHostedService<SimulatorWorker>();

var host = builder.Build();
host.Run();
