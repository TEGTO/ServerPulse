using Confluent.Kafka;
using MessageBus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using ServerMonitorApi.Infrastructure;
using ServerMonitorApi.Infrastructure.Settings;
using ServerMonitorApi.Settings;
using Testcontainers.Kafka;

namespace ServerMonitorApi.IntegrationTests
{
    public sealed class WebAppFactoryWrapper : IAsyncDisposable
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
            KafkaContainer = new KafkaBuilder()
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

                  builder.ConfigureTestServices(services =>
                  {
                      var adminConfig = new AdminClientConfig
                      {
                          BootstrapServers = KafkaContainer?.GetBootstrapAddress()
                      };

                      var consumerConfig = new ConsumerConfig
                      {
                          BootstrapServers = KafkaContainer?.GetBootstrapAddress(),
                          ClientId = "server-interaction",
                          GroupId = "server-interaction-group",
                          EnablePartitionEof = true,
                          AutoOffsetReset = AutoOffsetReset.Earliest
                      };

                      services.AddKafkaConsumer(consumerConfig, adminConfig);
                  });
              });
        }

        private IConfigurationRoot GetConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder();

            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                 { $"{KafkaSettings.SETTINGS_SECTION}:{nameof(KafkaSettings.BootstrapServers)}", KafkaContainer?.GetBootstrapAddress() },
                 { $"{KafkaSettings.SETTINGS_SECTION}:{nameof(KafkaSettings.ClientId)}", "server-interaction" },
                 { $"{MessageBusSettings.SETTINGS_SECTION}:{nameof(MessageBusSettings.AliveTopic)}", "AliveTopic_" },
                 { $"{MessageBusSettings.SETTINGS_SECTION}:{nameof(MessageBusSettings.ConfigurationTopic)}", "ConfigurationTopic_" },
                 { $"{MessageBusSettings.SETTINGS_SECTION}:{nameof(MessageBusSettings.LoadTopic)}", "LoadTopic_" },
                 { $"{MessageBusSettings.SETTINGS_SECTION}:{nameof(MessageBusSettings.LoadTopicProcess)}", "LoadEventProcessTopic" },
                 { $"{MessageBusSettings.SETTINGS_SECTION}:{nameof(MessageBusSettings.CustomTopic)}", "CustomEventTopic_" },
                 { Infrastructure.ConfigurationKeys.SERVER_SLOT_URL, "http://apigateway:8080" },
            });

            return configurationBuilder.Build();
        }
    }
}