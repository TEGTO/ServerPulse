using Confluent.Kafka;
using MessageBus;
using ServerMonitorApi.Infrastructure.Services;
using ServerMonitorApi.Infrastructure.Settings;
using Shared;

namespace ServerMonitorApi
{
    public static class HostApplicationBuilderExtensions
    {
        public static IHostApplicationBuilder AddInfrastructureServices(this IHostApplicationBuilder builder)
        {
            builder.Services.AddHttpClientHelperServiceWithResilience(builder.Configuration);
            var kafkaSettings = builder.Configuration.GetSection(KafkaSettings.SETTINGS_SECTION).Get<KafkaSettings>();

            ArgumentNullException.ThrowIfNull(kafkaSettings);

            builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection(KafkaSettings.SETTINGS_SECTION));

            #region Kafka

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = kafkaSettings.BootstrapServers,
                ClientId = kafkaSettings.ClientId,
                EnableIdempotence = true,
            };
            var adminConfig = new AdminClientConfig
            {
                BootstrapServers = kafkaSettings.BootstrapServers,
                AllowAutoCreateTopics = true
            };
            builder.Services.AddKafkaProducer(producerConfig, adminConfig);

            #endregion

            builder.Services.AddSingleton<ISlotKeyChecker, SlotKeyChecker>();

            return builder;
        }
    }
}
