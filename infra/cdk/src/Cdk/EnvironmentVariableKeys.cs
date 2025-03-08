namespace Cdk
{
    internal static class EnvironmentVariableKeys
    {
        public static string AspCoreEnvironment { get; } = "ASPNETCORE_ENVIRONMENT";

        public static string KafkaBootstrapServers { get; } = "Kafka__BootstrapServers";
        public static string KafkaClientIdAnalyzer { get; } = "Kafka__ClientId__Analyzer";
        public static string KafkaClientIdMonitorApi { get; } = "Kafka__ClientId__MonitorApi";
        public static string KafkaGroupId { get; } = "Kafka__GroupId";
        public static string KafkaPartitionsAmount { get; } = "Kafka__PartitionsAmount";

        public static string MessageBusReceiveTimeoutInMilliseconds { get; } = "MessageBus__ReceiveTimeoutInMilliseconds";
        public static string MessageBusAliveTopic { get; } = "MessageBus__AliveTopic";
        public static string MessageBusConfigurationTopic { get; } = "MessageBus__ConfigurationTopic";
        public static string MessageBusLoadTopic { get; } = "MessageBus__LoadTopic";
        public static string MessageBusLoadTopicProcess { get; } = "MessageBus__LoadTopicProcess";
        public static string MessageBusServerStatisticsTopic { get; } = "MessageBus__ServerStatisticsTopic";
        public static string MessageBusLoadMethodStatisticsTopic { get; } = "MessageBus__LoadMethodStatisticsTopic";
        public static string MessageBusCustomTopic { get; } = "MessageBus__CustomTopic";
        public static string MessageBusTopicDataSaveInDays { get; } = "MessageBus__TopicDataSaveInDays";

        public static string AuthSettingsKey { get; } = "AuthSettings__Key";
        public static string AuthSettingsIssuer { get; } = "AuthSettings__Issuer";
        public static string AuthSettingsExpiryInMinutes { get; } = "AuthSettings__ExpiryInMinutes";
        public static string AuthSettingsAudience { get; } = "AuthSettings__Audience";
        public static string AuthSettingsRefreshExpiryInDays { get; } = "AuthSettings__RefreshExpiryInDays";
        public static string AuthSettingsPublicKey { get; } = "AuthSettings__PublicKey";
        public static string AuthSettingsPrivateKey { get; } = "AuthSettings__PrivateKey";

        public static string EmailServiceType { get; } = "EmailServiceType";
        public static string EmailConnectionString { get; } = "Email__ConnectionString";
        public static string EmailSenderAddress1 { get; } = "Email__SenderAddress1";
        public static string EmailSenderAddress { get; } = "Email__SenderAddress";
        public static string EmailAccessKey { get; } = "Email__AccessKey";
        public static string EmailSecretKey { get; } = "Email__SecretKey";
        public static string EmailRegion { get; } = "Email__Region";

        public static string PostgresDb { get; } = "POSTGRES_DB";
        public static string PostgresUser { get; } = "POSTGRES_USER";
        public static string PostgresPassword { get; } = "POSTGRES_PASSWORD";

        public static string RedisPassword { get; } = "REDIS_PASSWORD";

        public static string ConnectionStringsAuthenticationDb { get; } = "ConnectionStrings__AuthenticationDb";
        public static string ConnectionStringsServerSlotDb { get; } = "ConnectionStrings__ServerSlotDb";
        public static string ConnectionStringsRedisServer { get; } = "ConnectionStrings__RedisServer";

        public static string CacheGetServerSlotByEmailExpiryInSeconds { get; } = "Cache__GetServerSlotByEmailExpiryInSeconds";
        public static string CacheServerSlotCheckExpiryInSeconds { get; } = "Cache__ServerSlotCheckExpiryInSeconds";
        public static string CacheServerSlotExpiryInMinutes { get; } = "Cache__ServerSlotExpiryInMinutes";

        public static string ApiGateway { get; } = "ApiGateway";
        public static string ServerSlotApiUrl { get; } = "ServerSlotApi__Url";
        public static string ServerSlotApiCheck { get; } = "ServerSlotApi__Check";

        public static string LoadEventProcessingBatchSize { get; } = "LoadEventProcessing__BatchSize";
        public static string LoadEventProcessingBatchIntervalInMilliseconds { get; } = "LoadEventProcessing__BatchIntervalInMilliseconds";

        public static string BackgroundServicesUseUserUnconfirmedCleanup { get; } = "BackgroundServices__UseUserUnconfirmedCleanup";
        public static string BackgroundServicesUnconfirmedUsersCleanUpInMinutes { get; } = "BackgroundServices__UnconfirmedUsersCleanUpInMinutes";

        public static string FeatureManagementOAuth { get; } = "FeatureManagement__OAuth";
        public static string FeatureManagementEmailConfirmation { get; } = "FeatureManagement__EmailConfirmation";

        public static string UseCORS { get; } = "UseCORS";
        public static string EFCreateDatabase { get; } = "EFCreateDatabase";
        public static string AllowedCORSOrigins { get; } = "AllowedCORSOrigins";

        public static string PulseEventIntervalInMilliseconds { get; } = "PulseEventIntervalInMilliseconds";
        public static string StatisticsCollectIntervalInMilliseconds { get; } = "StatisticsCollectIntervalInMilliseconds";
        public static string MinimumStatisticsTimeSpanInSeconds { get; } = "MinimumStatisticsTimeSpanInSeconds";
        public static string MaxEventAmountToGetInSlotData { get; } = "MaxEventAmountToGetInSlotData";
        public static string MaxEventAmountToReadPerRequest { get; } = "MaxEventAmountToReadPerRequest";
        public static string SlotsPerUser { get; } = "SlotsPerUser";
    }
}