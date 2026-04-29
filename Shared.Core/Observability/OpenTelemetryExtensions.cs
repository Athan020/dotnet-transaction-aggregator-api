using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Shared.Core.Observability;

public static class OpenTelemetryExtensions
{

    
    public static void AddObservability(this WebApplicationBuilder builder, string serviceName, IConfiguration configuration)
    {

        var jaegerEndpoint = configuration["Jaeger:Endpoint"];

        var otel = builder.Services.AddOpenTelemetry();


        otel.ConfigureResource(resource => resource
            .AddService(serviceName))
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation();
                metrics.AddHttpClientInstrumentation();

                metrics.AddMeter("Microsoft.EntityFrameworkCore");
                metrics.AddMeter("Microsoft.AspNetCore.Hosting");
                metrics.AddMeter("Microsoft.AspNetCore.Server.Kestrel");

                metrics.AddPrometheusExporter();

            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation();
                tracing.AddHttpClientInstrumentation();
                tracing.AddGrpcClientInstrumentation();
                tracing.AddEntityFrameworkCoreInstrumentation(opt =>
                {
                    opt.EnrichWithIDbCommand = (activity, command) =>
                    {
                        activity.SetTag("db.statement", command.CommandText);
                    };
                }
                );
                tracing.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(jaegerEndpoint!);
                    options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                });
            });

    }

    public static void AddObservability(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        AddObservability(builder, builder.Environment.ApplicationName, configuration);
    }

}
