using AnalyzerApi.Infrastructure.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.Kafka;
using Testcontainers.Redis;

namespace AnalyzerApi.IntegrationTests
{
    internal sealed class WebAppFactoryWrapper : IAsyncDisposable
    {
        private RedisContainer? RedisContainer { get; set; }
        private KafkaContainer? KafkaContainer { get; set; }
        private WebApplicationFactory<Program>? WebApplicationFactory { get; set; }

        public async Task<WebApplicationFactory<Program>> GetFactoryAsync()
        {
            if (WebApplicationFactory == null)
            {
                await InitializeContainersAsync();
                WebApplicationFactory = InitializeFactory();
            }
            return WebApplicationFactory;
        }

        public async ValueTask DisposeAsync()
        {
            if (RedisContainer != null)
            {
                await RedisContainer.StopAsync();
                await RedisContainer.DisposeAsync();
            }

            if (KafkaContainer != null)
            {
                await KafkaContainer.StopAsync();
                await KafkaContainer.DisposeAsync();
            }

            if (WebApplicationFactory != null)
            {
                await WebApplicationFactory.DisposeAsync();
                WebApplicationFactory = null;
            }
        }

        private async Task InitializeContainersAsync()
        {
            RedisContainer = new RedisBuilder()
                .WithImage("redis:7.4")
                .Build();

            var kafkaContainerName = Guid.NewGuid().ToString("D");

            KafkaContainer = new KafkaBuilder()
                .WithName(kafkaContainerName)
                .WithImage("confluentinc/cp-kafka:7.5.0")
                .WithEnvironment("KAFKA_NUM_PARTITIONS", "3")
                .Build();

            await RedisContainer.StartAsync();
            await KafkaContainer.StartAsync();
        }

        private WebApplicationFactory<Program> InitializeFactory()
        {
            return new WebApplicationFactory<Program>()
              .WithWebHostBuilder(builder =>
              {
                  builder.UseConfiguration(GetConfiguration());
              });
        }

        private IConfigurationRoot GetConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder();

            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                 { $"{MessageBusSettings.SETTINGS_SECTION}:{nameof(MessageBusSettings.BootstrapServers)}", KafkaContainer?.GetBootstrapAddress() },
                 { $"{MessageBusSettings.SETTINGS_SECTION}:{nameof(MessageBusSettings.ClientId)}", "analyzer" },
                 { $"{MessageBusSettings.SETTINGS_SECTION}:{nameof(MessageBusSettings.GroupId)}", "analyzer-group" },
                 { $"{MessageBusSettings.SETTINGS_SECTION}:{nameof(MessageBusSettings.ReceiveTimeoutInMilliseconds)}", "10000" },
                 { $"{MessageBusSettings.SETTINGS_SECTION}:{nameof(MessageBusSettings.AliveTopic)}", "AliveTopic_" },
                 { $"{MessageBusSettings.SETTINGS_SECTION}:{nameof(MessageBusSettings.ConfigurationTopic)}", "ConfigurationTopic_" },
                 { $"{MessageBusSettings.SETTINGS_SECTION}:{nameof(MessageBusSettings.LoadTopic)}", "LoadTopic_" },
                 { $"{MessageBusSettings.SETTINGS_SECTION}:{nameof(MessageBusSettings.LoadTopicProcess)}", "LoadEventProcessTopic" },
                 { $"{MessageBusSettings.SETTINGS_SECTION}:{nameof(MessageBusSettings.CustomTopic)}", "CustomEventTopic_" },
                 { $"{MessageBusSettings.SETTINGS_SECTION}:{nameof(MessageBusSettings.LoadMethodStatisticsTopic)}", "LoadMethodStatisticsTopic_" },
                 { $"{MessageBusSettings.SETTINGS_SECTION}:{nameof(MessageBusSettings.TopicDataSaveInDays)}", "365" },
                 { $"ConnectionStrings:{CacheSettings.REDIS_SERVER_CONNECTION_STRING}",  RedisContainer?.GetConnectionString()},
                 { $"{CacheSettings.SETTINGS_SECTION}:{nameof(CacheSettings.GetDailyLoadAmountStatisticsExpiryInMinutes)}", "5" },
                 { $"{CacheSettings.SETTINGS_SECTION}:{nameof(CacheSettings.GetLoadAmountStatisticsInRangeExpiryInMinutes)}", "5" },
                 { $"{CacheSettings.SETTINGS_SECTION}:{nameof(CacheSettings.GetLoadEventsInDataRangeExpiryInMinutes)}", "5" },
                 { $"{CacheSettings.SETTINGS_SECTION}:{nameof(CacheSettings.GetSlotStatisticsExpiryInMinutes)}", "5" },
                 { $"{LoadProcessingSettings.SETTINGS_SECTION}:{nameof(LoadProcessingSettings.BatchSize)}", "20" },
                 { $"{LoadProcessingSettings.SETTINGS_SECTION}:{nameof(LoadProcessingSettings.BatchIntervalInMilliseconds)}", "1000" },
                 { ConfigurationKeys.STATISTICS_COLLECT_INTERVAL_IN_MILLISECONDS, "500" },
                 { ConfigurationKeys.MIN_STATISTICS_TIMESPAN_IN_SECONDS, "0" },
                 { ConfigurationKeys.MAX_EVENT_AMOUNT_TO_GET_IN_SLOT_DATA, "25" },
                 { ConfigurationKeys.MAX_EVENT_AMOUNT_PER_REQUEST, "20" },
            });

            return configurationBuilder.Build();
        }
    }
}