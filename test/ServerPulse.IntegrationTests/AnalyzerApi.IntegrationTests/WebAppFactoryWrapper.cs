using AnalyzerApi.Infrastructure;
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
                 { $"ConnectionStrings:{Configuration.REDIS_SERVER_CONNECTION_STRING}",  RedisContainer?.GetConnectionString()},
                 { Configuration.KAFKA_CLIENT_ID, "analyzer" },
                 { Configuration.KAFKA_BOOTSTRAP_SERVERS, KafkaContainer?.GetBootstrapAddress() },
                 { Configuration.KAFKA_GROUP_ID, "analyzer-group" },
                 { Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS, "10000" },
                 { Configuration.KAFKA_TOPIC_DATA_SAVE_IN_DAYS, "365" },
                 { Configuration.KAFKA_ALIVE_TOPIC, "AliveTopic_" },
                 { Configuration.KAFKA_CONFIGURATION_TOPIC, "ConfigurationTopic_" },
                 { Configuration.KAFKA_LOAD_TOPIC, "LoadTopic_" },
                 { Configuration.KAFKA_LOAD_TOPIC_PROCESS, "LoadEventProcessTopic" },
                 { Configuration.KAFKA_CUSTOM_TOPIC, "CustomEventTopic_" },
                 { Configuration.KAFKA_LOAD_METHOD_STATISTICS_TOPIC, "LoadMethodStatisticsTopic_" },
                 { Configuration.STATISTICS_COLLECT_INTERVAL_IN_MILLISECONDS, "500" },
                 { Configuration.CACHE_EXPIRY_IN_MINUTES, "5" },
                 { Configuration.MIN_STATISTICS_TIMESPAN_IN_SECONDS, "0" },
                 { Configuration.MAX_EVENT_AMOUNT_TO_GET_IN_SLOT_DATA, "25" },
                 { Configuration.MAX_EVENT_AMOUNT_PER_REQUEST, "20" },
                 { Configuration.LOAD_EVENT_PROCESSING_BATCH_SIZE, "20" },
                 { Configuration.LOAD_EVENT_PROCESSING_BATCH_INTERVAL_IN_MILLISECONDS, "1000" },
            });

            return configurationBuilder.Build();
        }
    }
}