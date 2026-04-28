using System;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace Transaction.Aggregator.Api.Extensions;

public static class DistributedCacheExtensions
{

    public static TBuilder AddDistributedCache<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("Redis");
            options.InstanceName = "TransactionAggregatorCache_";
        });

        builder.Services.AddFusionCache()
            .WithDefaultEntryOptions(new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromMinutes(5),
                IsFailSafeEnabled = true,
                FailSafeMaxDuration = TimeSpan.FromMinutes(10),
            })
            .WithSerializer(new FusionCacheSystemTextJsonSerializer())
            .TryWithRegisteredDistributedCache()
            .WithRegisteredLogger()
            .AsHybridCache();

        return builder;
    }
}
