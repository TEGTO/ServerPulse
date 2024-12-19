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
                .WithImage("redis:latest")
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
                { "Kafka:BootstrapServers", KafkaContainer?.GetBootstrapAddress() },
                { "Kafka:ClientId", "analyzer" },
                { "Kafka:GroupId", "analyzer-group" },
                { "Kafka:AnalyzerReceiveTimeout", "10000" },
                { "Kafka:TopicDataSaveInDays", "365" },
                { "Kafka:AliveTopic", "AliveTopic_" },
                { "Kafka:ConfigurationTopic", "ConfigurationTopic_" },
                { "Kafka:LoadTopic", "LoadTopic_" },
                { "Kafka:ProcessLoadEventTopic", "LoadEventProcessTopic" },
                { "Kafka:CustomTopic", "CustomEventTopic_" },
                { "Kafka:LoadMethodStatisticsTopic", "LoadMethodStatisticsTopic_" },
                { "PulseEventIntervalInMilliseconds", "20000" },
                { "StatisticsCollectIntervalInMilliseconds", "500" },
                { "ConnectionStrings:RedisServer",  RedisContainer?.GetConnectionString()},
                { "Cache:Cache__ExpiryInMinutes", "5" },
                { "MinimumStatisticsTimeSpanInSeconds", "0" },
                { "MaxEventAmountToGetInSlotData", "25" },
                { "MaxEventAmountToReadPerRequest", "20" },
                { "LoadEventProcessing:BatchSize", "20" },
                { "LoadEventProcessing:BatchIntervalInMilliseconds", "1000" },
            });

            return configurationBuilder.Build();
        }
    }
}