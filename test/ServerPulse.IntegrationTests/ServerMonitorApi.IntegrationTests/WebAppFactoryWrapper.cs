using Confluent.Kafka;
using Consul;
using MessageBus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using Testcontainers.Consul;
using Testcontainers.Kafka;
using Testcontainers.Redis;

namespace ServerMonitorApi.IntegrationTests
{
    public class WebAppFactoryWrapper : IAsyncDisposable
    {
        private ConsulContainer consulContainer;
        private RedisContainer redisContainer;
        private KafkaContainer kafkaContainer;

        public WebApplicationFactory<Program> WebApplicationFactory { get; private set; }

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
                      var adminConfig = new AdminClientConfig
                      {
                          BootstrapServers = kafkaContainer.GetBootstrapAddress()
                      };
                      var consumerConfig = new ConsumerConfig
                      {
                          BootstrapServers = kafkaContainer.GetBootstrapAddress(),
                          ClientId = "server-interaction",
                          GroupId = "server-interaction-group",
                          EnablePartitionEof = true,
                          AutoOffsetReset = AutoOffsetReset.Earliest
                      };
                      services.AddKafkaConsumer(consumerConfig, adminConfig);
                  });
              });
        }
        private async Task PopulateConsulAsync()
        {
            var consulClient = new ConsulClient(config => config.Address = new Uri(consulContainer.GetBaseAddress()));

            var kvPairs = new Dictionary<string, object>
            {
                { "Kafka:ClientId", "server-interaction" },
                { "Kafka:BootstrapServers", kafkaContainer.GetBootstrapAddress() },
                { "Kafka:AliveTopic", "AliveTopic_" },
                { "Kafka:ConfigurationTopic", "ConfigurationTopic_" },
                { "Kafka:LoadTopic", "LoadTopic_" },
                { "Kafka:CustomTopic", "CustomEventTopic_" },
                { "ApiGateway", "http://apigateway:8080" },
                { "ServerSlotApi:Check", "/serverslot/check" },
                { "Analyzer:Load", "/eventprocessing/load" },
                { "ConnectionStrings:RedisServer", redisContainer.GetConnectionString() },
                { "Cache:ServerSlotExpiryInMinutes", "30" },
            };

            var developmentJson = JsonSerializer.Serialize(kvPairs);
            var productionJson = JsonSerializer.Serialize(kvPairs);

            await consulClient.KV.Put(new KVPair("server-monitor/appsettings.Development.json")
            {
                Value = Encoding.UTF8.GetBytes(developmentJson)
            });

            await consulClient.KV.Put(new KVPair("server-monitor/appsettings.Production.json")
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
                { "Consul:ServiceName", "server-monitor" },
                { "Consul:ServicePort", "80" }
            });
            return configurationBuilder.Build();
        }
    }
}