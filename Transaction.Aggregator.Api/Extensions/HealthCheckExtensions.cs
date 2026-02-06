using System;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Transaction.Aggregator.Api.Extensions;

public static class HealthCheckExtensions
{
    public static TBuilder AddCustomHealthChecks<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services
            .AddHealthChecks()
            .AddRedis(builder.Configuration.GetConnectionString("Redis")!)
            .AddCheck("self", () => HealthCheckResult.Healthy("API is Available"),
                tags: ["ready"]);

        return builder;
    }

}
