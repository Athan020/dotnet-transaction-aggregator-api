using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Simmy;
using Polly.Timeout;
using Transaction.Aggregator.Application.Configuration;

namespace Transaction.Aggregator.Application;

public class ResiliencePipelineFactory(IOptions<Settings> options) : IResiliencePipelineFactory
{
    private static readonly ConcurrentDictionary<string, ResiliencePipeline> Pipelines = new();

    private readonly Settings _settings = options.Value;

    public  ResiliencePipeline GetOrCreatePipeline(string source)
    {
        return Pipelines.GetOrAdd(source, Create);
    }
    private ResiliencePipeline Create(string source)
    {
        _settings.TryGetValue("EnableChaos", out var enableChaos);

        var builder = new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                ShouldHandle = new PredicateBuilder().Handle<TimeoutRejectedException>(),
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromMilliseconds(200),
                UseJitter = true,
                MaxRetryAttempts = 2
            })
            .AddTimeout(TimeSpan.FromSeconds(2))
            .AddCircuitBreaker(new()
            {
                ShouldHandle = new PredicateBuilder()
                            .Handle<Exception>(),
                BreakDuration = TimeSpan.FromSeconds(10),
                FailureRatio = 0.5,
                MinimumThroughput = 5,      
            });

        if (enableChaos)
        {
            builder
                .AddChaosLatency(0.3, TimeSpan.FromSeconds(4));
        }


        return builder.Build();
    }
}

public interface IResiliencePipelineFactory
{
    ResiliencePipeline GetOrCreatePipeline(string source);
}