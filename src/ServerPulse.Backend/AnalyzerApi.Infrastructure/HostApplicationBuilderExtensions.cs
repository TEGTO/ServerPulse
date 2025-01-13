using AnalyzerApi.Application.Application.Services;
using AnalyzerApi.Core.Dtos.Endpoints.Analyze.GetLoadAmountStatisticsInRange;
using AnalyzerApi.Core.Dtos.Endpoints.Analyze.GetLoadEventsInDataRange;
using AnalyzerApi.Core.Dtos.Responses.Statistics;
using AnalyzerApi.Core.Models.Statistics;
using AnalyzerApi.Infrastructure.Services;
using AnalyzerApi.Infrastructure.Settings;
using Caching;
using Confluent.Kafka;
using MessageBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AnalyzerApi.Infrastructure
{
    public static class HostApplicationBuilderExtensions
    {
        public static IHostApplicationBuilder AddInfrastructureServices(this IHostApplicationBuilder builder)
        {
            #region Options

            var kafkaSettings = builder.Configuration.GetSection(KafkaSettings.SETTINGS_SECTION).Get<KafkaSettings>();

            ArgumentNullException.ThrowIfNull(kafkaSettings);

            builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection(KafkaSettings.SETTINGS_SECTION));

            var cacheSettings = builder.Configuration.GetSection(CacheSettings.SETTINGS_SECTION).Get<CacheSettings>();

            ArgumentNullException.ThrowIfNull(cacheSettings);

            builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection(CacheSettings.SETTINGS_SECTION));

            #endregion

            builder.Services.AddSignalR();

            #region Kafka

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = kafkaSettings.BootstrapServers,
                ClientId = kafkaSettings.ClientId,
                GroupId = kafkaSettings.GroupId,
                EnablePartitionEof = true,
                AutoOffsetReset = AutoOffsetReset.Earliest,
            };

            var adminConfig = new AdminClientConfig
            {
                BootstrapServers = kafkaSettings.BootstrapServers,
            };

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = kafkaSettings.BootstrapServers,
                ClientId = kafkaSettings.ClientId,
                EnableIdempotence = true,
            };
            builder.Services.AddKafkaProducer(producerConfig, adminConfig);
            builder.Services.AddKafkaConsumer(consumerConfig, adminConfig);

            #endregion

            #region Caching

            builder.Services.AddStackExchangeRedisOutputCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString(CacheSettings.REDIS_SERVER_CONNECTION_STRING);
            });

            builder.Services.AddOutputCache((options) =>
            {
                options.AddPolicy("BasePolicy", new OutputCachePolicy());

                options.SetOutputCachePolicy("GetLoadEventsInDataRangePolicy", duration: TimeSpan.FromMinutes(cacheSettings.GetLoadEventsInDataRangeExpiryInMinutes), types: typeof(GetLoadEventsInDataRangeRequest));
                options.SetOutputCachePolicy("GetDailyLoadAmountStatisticsPolicy", duration: TimeSpan.FromMinutes(cacheSettings.GetDailyLoadAmountStatisticsExpiryInMinutes));
                options.SetOutputCachePolicy("GetLoadAmountStatisticsInRangePolicy", duration: TimeSpan.FromMinutes(cacheSettings.GetLoadAmountStatisticsInRangeExpiryInMinutes), types: typeof(GetLoadAmountStatisticsInRangeRequest));
                options.SetOutputCachePolicy("GetSlotStatisticsPolicy", duration: TimeSpan.FromMinutes(cacheSettings.GetSlotStatisticsExpiryInMinutes));
            });

            #endregion

            #region Project Services

            builder.Services.AddSingleton<IStatisticsNotifier<ServerLifecycleStatistics, ServerLifecycleStatisticsResponse>, SignalRStatisticsNotifier<ServerLifecycleStatistics, ServerLifecycleStatisticsResponse>>();
            builder.Services.AddSingleton<IStatisticsNotifier<ServerLoadStatistics, ServerLoadStatisticsResponse>, SignalRStatisticsNotifier<ServerLoadStatistics, ServerLoadStatisticsResponse>>();
            builder.Services.AddSingleton<IStatisticsNotifier<ServerCustomStatistics, ServerCustomStatisticsResponse>, SignalRStatisticsNotifier<ServerCustomStatistics, ServerCustomStatisticsResponse>>();

            #endregion

            return builder;
        }
    }
}
