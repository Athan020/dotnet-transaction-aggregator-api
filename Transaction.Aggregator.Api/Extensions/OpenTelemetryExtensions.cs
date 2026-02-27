using System;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Transaction.Aggregator.Api.Extensions;

public static class OpenTelemetryExtensions
{

    public static TBuilder AddOpenTelemetry<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        var jaegerEndpoint = builder.Configuration.GetValue<string>("OpenTelemetry:JaegerEndpoint") 
            ?? Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") 
            ?? "http://localhost:4318";
        
        var prometheusPort = builder.Configuration.GetValue<int?>("OpenTelemetry:PrometheusPort") ?? 9090;

        // Logging
        builder.Logging.AddOpenTelemetry(options =>
        {
            options
                .SetResourceBuilder(CreateResourceBuilder(builder.Environment))
                .AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(jaegerEndpoint);
                });

            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
        });

        // Tracing
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(CreateResourceBuilder(builder.Environment))
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequest = (activity, request) =>
                        {
                            activity.SetTag("request.headers.user-agent", request.Headers.UserAgent);
                        };
                        options.EnrichWithHttpResponse = (activity, response) =>
                        {
                            activity.SetTag("response.content-type", response.ContentType);
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(jaegerEndpoint);
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(CreateResourceBuilder(builder.Environment))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    // .AddProcessInstrumentation()
                    .AddMeter("TransactionAggregatorApi");
                    // metrics are exposed via prometheus-net instead of the OTLP Prometheus exporter
            });

        return builder;
    }


    private static ResourceBuilder CreateResourceBuilder(IHostEnvironment environment)
    {
        return ResourceBuilder.CreateDefault()
            .AddService("TransactionAggregatorApi", serviceVersion: "1.0.0")
            .AddTelemetrySdk()
            .AddAttributes(new[] {
                new KeyValuePair<string, object>("environment", environment.EnvironmentName),
                new KeyValuePair<string, object>("application.name", "Transaction.Aggregator.Api"),
                new KeyValuePair<string, object>("service.namespace", "aggregator")
            });
    }
}
