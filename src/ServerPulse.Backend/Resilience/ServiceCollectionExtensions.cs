using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Resilience
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultResiliencePipeline(this IServiceCollection services,
            IConfiguration configuration, string defaultName = "Default")
        {
            var pipelineConfiguration = configuration.GetSection(ConfigurationKeys.DEFAULT_RESILIENCE_PIPELINE_SECTION)
                                        .Get<ResiliencePipelineSettings>() ?? new ResiliencePipelineSettings();

            services.AddResiliencePipeline(defaultName, (builder, context) =>
            {
                ResilienceHelpers.ConfigureResiliencePipeline(builder, context, pipelineConfiguration);
            });

            return services;
        }
    }
}
