using Confluent.Kafka;
using MessageBus;
using Refit;
using ServerMonitorApi.Infrastructure;
using ServerMonitorApi.Infrastructure.Services;
using ServerMonitorApi.Infrastructure.Settings;
using System.Net.Http.Headers;

namespace ServerMonitorApi
{
    public static class HostApplicationBuilderExtensions
    {
        public static IHostApplicationBuilder AddInfrastructureServices(this IHostApplicationBuilder builder)
        {
            var kafkaSettings = builder.Configuration.GetSection(KafkaSettings.SETTINGS_SECTION).Get<KafkaSettings>();

            builder.Services.AddRefitClient<IServerSlotApi>()
              .ConfigureHttpClient((sp, httpClient) =>
              {
                  httpClient.BaseAddress = new Uri(builder.Configuration[ConfigurationKeys.SERVER_SLOT_URL]!);
                  httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
              }).AddStandardResilienceHandler();

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
