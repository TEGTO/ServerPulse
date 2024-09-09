using Consul;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using Testcontainers.Consul;
using Testcontainers.Kafka;
using Testcontainers.Redis;

namespace AnalyzerApi.IntegrationTests
{
    public class WebAppFactoryWrapper : IAsyncDisposable
    {
        private ConsulContainer consulContainer;
        private RedisContainer redisContainer;
        private KafkaContainer kafkaContainer;

        protected WebApplicationFactory<Program> WebApplicationFactory { get; private set; }

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
            if (WebApplicationFactory != null)
            {
                await consulContainer.StopAsync();
                await redisContainer.StopAsync();
                await kafkaContainer.StopAsync();

                await consulContainer.DisposeAsync();
                await redisContainer.DisposeAsync();
                await kafkaContainer.DisposeAsync();

                await WebApplicationFactory.DisposeAsync();
            }
            GC.SuppressFinalize(this);
        }

        private async Task InitializeContainersAsync()
        {
            consulContainer = new ConsulBuilder()
                .WithImage("hashicorp/consul:latest")
                .Build();

            redisContainer = new RedisBuilder()
                .WithImage("redis:latest")
                .Build();

            kafkaContainer = new KafkaBuilder()
                .WithImage("confluentinc/cp-kafka:7.5.0")
                .WithEnvironment("KAFKA_NUM_PARTITIONS", "3")
                .Build();

            await consulContainer.StartAsync();
            await redisContainer.StartAsync();
            await kafkaContainer.StartAsync();

            await PopulateConsulAsync();
        }
        private WebApplicationFactory<Program> InitializeFactory()
        {
            return new WebApplicationFactory<Program>()
              .WithWebHostBuilder(builder =>
              {
                  builder.UseConfiguration(GetConfiguration());

                  builder.ConfigureTestServices(services =>
                  {

                  });
              });
        }
        private async Task PopulateConsulAsync()
        {
            var consulClient = new ConsulClient(config => config.Address = new Uri(consulContainer.GetBaseAddress()));

            var kvPairs = new Dictionary<string, object>
            {
                { "Kafka:BootstrapServers", kafkaContainer.GetBootstrapAddress() },
                { "Kafka:ClientId", "analyzer" },
                { "Kafka:GroupId", "analyzer-group" },
                { "Kafka:AnalyzerReceiveTimeout", "5000" },
                { "Kafka:TopicDataSaveInDays", "365" },
                { "Kafka:AliveTopic", "AliveTopic_" },
                { "Kafka:ConfigurationTopic", "ConfigurationTopic_" },
                { "Kafka:LoadTopic", "LoadTopic_" },
                { "Kafka:CustomTopic", "CustomEventTopic_" },
                { "Kafka:LoadMethodStatisticsTopic", "LoadMethodStatisticsTopic_" },
                { "PulseEventIntervalInMilliseconds", "20000" },
                { "StatisticsCollectIntervalInMilliseconds", "5000" },
                { "ConnectionStrings:RedisServer",  redisContainer.GetConnectionString()},
                { "Cache:ServerLoadStatisticsPerDayExpiryInMinutes", "60" },
                { "Cache:StatisticsKey", "StatisticsPerday-" },
                { "MinimumStatisticsTimeSpanInSeconds", "0" },
                { "MaxEventAmountToGetInSlotData", "25" },
            };

            var developmentJson = JsonSerializer.Serialize(kvPairs);
            var productionJson = JsonSerializer.Serialize(kvPairs);

            await consulClient.KV.Put(new KVPair("analyzer/appsettings.Development.json")
            {
                Value = Encoding.UTF8.GetBytes(developmentJson)
            });

            await consulClient.KV.Put(new KVPair("analyzer/appsettings.Production.json")
            {
                Value = Encoding.UTF8.GetBytes(productionJson)
            });
        }
        private IConfigurationRoot GetConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder();
            var s = consulContainer.GetBaseAddress();
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Consul:Host", consulContainer.GetBaseAddress() },
                { "Consul:ServiceName", "analyzer" },
                { "Consul:ServicePort", "80" }
            });
            return configurationBuilder.Build();
        }
    }
}