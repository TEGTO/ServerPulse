using Amazon.CDK;

namespace Cdk
{
    public sealed class ServerPulseStackProps : StackProps
    {
        public required string AspCoreEnvironment { get; set; }

        public string KafkaBootstrapServers { get; set; }
        public required string KafkaClientIdAnalyzer { get; set; }
        public required string KafkaClientIdMonitorApi { get; set; }
        public required string KafkaGroupId { get; set; }
        public required int KafkaPartitionsAmount { get; set; }

        public required int MessageBusReceiveTimeoutInMilliseconds { get; set; }
        public required string MessageBusAliveTopic { get; set; }
        public required string MessageBusConfigurationTopic { get; set; }
        public required string MessageBusLoadTopic { get; set; }
        public required string MessageBusLoadTopicProcess { get; set; }
        public required string MessageBusServerStatisticsTopic { get; set; }
        public string MessageBusLoadMethodStatisticsTopic { get; set; }
        public required string MessageBusCustomTopic { get; set; }
        public required int MessageBusTopicDataSaveInDays { get; set; }

        public required string AuthSettingsKey { get; set; }
        public required string AuthSettingsIssuer { get; set; }
        public required double AuthSettingsExpiryInMinutes { get; set; }
        public required string AuthSettingsAudience { get; set; }
        public required int AuthSettingsRefreshExpiryInDays { get; set; }
        public required string AuthSettingsPublicKey { get; set; }
        public required string AuthSettingsPrivateKey { get; set; }

        public string EmailConnectionString { get; set; }
        public string EmailSenderAddress { get; set; }
        public string EmailAccessKey { get; set; }
        public string EmailSecretKey { get; set; }
        public string EmailRegion { get; set; }

        public required string PostgresDb { get; set; }
        public required string PostgresUser { get; set; }
        public required string PostgresPassword { get; set; }

        public required string RedisPassword { get; set; }

        public string ConnectionStringsAuthenticationDb { get; set; }
        public string ConnectionStringsServerSlotDb { get; set; }
        public string ConnectionStringsRedisServer { get; set; }

        public required double CacheGetServerSlotByEmailExpiryInSeconds { get; set; }
        public required double CacheServerSlotCheckExpiryInSeconds { get; set; }
        public required double CacheServerSlotExpiryInMinutes { get; set; }
        public required double CacheGetLoadEventsInDataRangeExpiryInMinutes { get; set; }
        public required double CacheGetDailyLoadAmountStatisticsExpiryInMinutes { get; set; }
        public required double CacheGetLoadAmountStatisticsInRangeExpiryInMinutes { get; set; }
        public required double CacheGetSlotStatisticsExpiryInMinutes { get; set; }

        public string ApiGateway { get; set; }
        public string ServerSlotApiUrl { get; set; }
        public required string ServerSlotApiCheck { get; set; }

        public required int LoadEventProcessingBatchSize { get; set; }
        public required int LoadEventProcessingBatchIntervalInMilliseconds { get; set; }

        public required bool BackgroundServicesUseUserUnconfirmedCleanup { get; set; }
        public required double BackgroundServicesUnconfirmedUsersCleanUpInMinutes { get; set; }

        public required bool FeatureManagementOAuth { get; set; }
        public required bool FeatureManagementEmailConfirmation { get; set; }

        public required bool UseCORS { get; set; }
        public required bool EFCreateDatabase { get; set; }
        public required string AllowedCORSOrigins { get; set; }

        public required int PulseEventIntervalInMilliseconds { get; set; }
        public required int StatisticsCollectIntervalInMilliseconds { get; set; }
        public required double MinimumStatisticsTimeSpanInSeconds { get; set; }
        public required int MaxEventAmountToGetInSlotData { get; set; }
        public required int MaxEventAmountToReadPerRequest { get; set; }
        public required int SlotsPerUser { get; set; }
    }
}