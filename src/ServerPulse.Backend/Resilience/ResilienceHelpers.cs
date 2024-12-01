using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.DependencyInjection;

namespace Resilience
{
    public static class ResilienceHelpers
    {
        public static void ConfigureResiliencePipeline(ResiliencePipelineBuilder builder, AddResiliencePipelineContext<string> context, ResiliencePipelineConfiguration config)
        {
            builder.AddRetry(new()
            {
                Delay = TimeSpan.FromSeconds(config.RetryWaitInSeconds),
                MaxRetryAttempts = config.MaxRetryCount,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    var logger = context.ServiceProvider.GetService<ILogger<ResiliencePipelineBuilder>>();
                    var str = $"Retry {args.AttemptNumber} due to {args.Outcome.Exception?.Message}. Waiting {args.Duration.TotalSeconds} seconds before next retry.";
                    logger?.LogWarning(str);
                    return default;
                }
            })
            .AddCircuitBreaker(new()
            {
                FailureRatio = config.CircuitFailureRatio,
                MinimumThroughput = config.CircuitMinimumThroughput,
                BreakDuration = TimeSpan.FromSeconds(config.CircuitWaitInSeconds),
                OnOpened = args =>
                {
                    var logger = context.ServiceProvider.GetService<ILogger<ResiliencePipelineBuilder>>();
                    var str = $"Circuit breaker triggered. Circuit will be open for {args.BreakDuration.TotalSeconds} seconds due to {args.Outcome.Exception?.Message}.";
                    logger?.LogWarning(str);
                    return default;
                },
                OnClosed = args =>
                {
                    var logger = context.ServiceProvider.GetService<ILogger<ResiliencePipelineBuilder>>();
                    var str = "Circuit breaker closed.";
                    logger?.LogWarning(str);
                    return default;
                }
            });
        }
    }
}
