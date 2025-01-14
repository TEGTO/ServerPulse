using AnalyzerApi.Application.Configuration;
using AnalyzerApi.Infrastructure.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.Kafka;

namespace AnalyzerApi.LoadEventStatisticsProcessor.IntegrationTests
{
    internal sealed class WebAppFactoryWrapper : IAsyncDisposable
    {
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
            var kafkaContainerName = Guid.NewGuid().ToString("D");

            KafkaContainer = new KafkaBuilder()
                .WithName(kafkaContainerName)
                .WithImage("confluentinc/cp-kafka:7.5.0")
                .WithEnvironment("KAFKA_NUM_PARTITIONS", "3")
                .Build();

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
                 { $"{KafkaSettings.SETTINGS_SECTION}:{nameof(KafkaSettings.BootstrapServers)}", KafkaContainer?.GetBootstrapAddress() },
                 { $"{KafkaSettings.SETTINGS_SECTION}:{nameof(KafkaSettings.ClientId)}", "analyzer" },
                 { $"{KafkaSettings.SETTINGS_SECTION}:{nameof(KafkaSettings.GroupId)}", "analyzer-group" },
                 { $"{MessageBusSettings.SETTINGS_SECTION}:{nameof(MessageBusSettings.ReceiveTimeoutInMilliseconds)}", "10000" },
                 { $"{MessageBusSettings.SETTINGS_SECTION}:{nameof(MessageBusSettings.LoadTopic)}", "LoadTopic_" },
                 { $"{MessageBusSettings.SETTINGS_SECTION}:{nameof(MessageBusSettings.LoadTopicProcess)}", "LoadEventProcessTopic" },
                 { $"{MessageBusSettings.SETTINGS_SECTION}:{nameof(MessageBusSettings.LoadMethodStatisticsTopic)}", "LoadMethodStatisticsTopic_" },
                 { $"ConnectionStrings:{CacheSettings.REDIS_SERVER_CONNECTION_STRING}",  "" },
                 { $"{CacheSettings.SETTINGS_SECTION}:{nameof(CacheSettings.GetDailyLoadAmountStatisticsExpiryInMinutes)}", "5" },
                 { $"{CacheSettings.SETTINGS_SECTION}:{nameof(CacheSettings.GetLoadAmountStatisticsInRangeExpiryInMinutes)}", "5" },
                 { $"{CacheSettings.SETTINGS_SECTION}:{nameof(CacheSettings.GetLoadEventsInDataRangeExpiryInMinutes)}", "5" },
                 { $"{CacheSettings.SETTINGS_SECTION}:{nameof(CacheSettings.GetSlotStatisticsExpiryInMinutes)}", "5" },
                 { $"{LoadProcessingSettings.SETTINGS_SECTION}:{nameof(LoadProcessingSettings.BatchSize)}", "20" },
                 { $"{LoadProcessingSettings.SETTINGS_SECTION}:{nameof(LoadProcessingSettings.BatchIntervalInMilliseconds)}", "1000" },
            });

            return configurationBuilder.Build();
        }
    }
}