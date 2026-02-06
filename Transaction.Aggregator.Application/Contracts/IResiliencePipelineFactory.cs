using Polly;

namespace Transaction.Aggregator.Application;

public interface IResiliencePipelineFactory
{
    ResiliencePipeline GetOrCreatePipeline(string source);
}