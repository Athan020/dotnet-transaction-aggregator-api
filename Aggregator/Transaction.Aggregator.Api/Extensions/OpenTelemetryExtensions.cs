// using System;
// using OpenTelemetry.Logs;
// using OpenTelemetry.Metrics;
// using OpenTelemetry.Resources;
// using OpenTelemetry.Trace;

// namespace Transaction.Aggregator.Api.Extensions;

// public static class OpenTelemetryExtensions
// {

//     public static TBuilder AddOpenTelemetry<TBuilder>(this TBuilder builder)
//         where TBuilder : IHostApplicationBuilder
//     {

//         builder.Logging.AddOpenTelemetry(options =>
//         {
//             options
//                 .AddConsoleExporter();

//             options.IncludeFormattedMessage = true;
//             options.IncludeScopes = true;

//             options.SetResourceBuilder(CreateResourceBuilder(builder.Environment));
//         });

//         builder.Services.AddOpenTelemetry()
//             .WithMetrics(options =>
//             {
//                 options
//                     .SetResourceBuilder(CreateResourceBuilder(builder.Environment))
//                     .AddAspNetCoreInstrumentation()
//                     .AddConsoleExporter();
//             });

//         return builder;
//     }


//     private static ResourceBuilder CreateResourceBuilder(IHostEnvironment environment)
//     {
//         return ResourceBuilder.CreateDefault()
//             .AddService("TransactionAggregatorApi")
//             .AddAttributes([
//                 new KeyValuePair<string, object>("development.environment", environment.EnvironmentName)
//             ]);
//     }
// }
