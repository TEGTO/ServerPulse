using Amazon.CDK;

namespace Cdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();

            var images = new DockerImages
            {
                AuthenticationApi = "tegto/serverpulse.authenticationapi:dev",
                ServerMonitorApi = "tegto/serverpulse.servermonitorapi:dev",
                AnalyzerApi = "tegto/serverpulse.analyzerapi:dev",
                ServerSlotApi = "tegto/serverpulse.serverslotapi:dev",
                ApiGateway = "tegto/serverpulse.apigateway:dev",
                Frontend = "tegto/serverpulse.frontend:dev",
            };

            var props = new ServerPulseStackProps
            {
                AspCoreEnvironment = "Development",

                KafkaPartitionsAmount = 3,
                KafkaClientIdAnalyzer = "analyzer",
                KafkaClientIdMonitorApi = "server-interaction",
                KafkaGroupId = "analyzer-group",

                MessageBusReceiveTimeoutInMilliseconds = 5000,
                MessageBusAliveTopic = "AliveTopic_",
                MessageBusConfigurationTopic = "ConfigurationTopic_",
                MessageBusLoadTopic = "LoadTopic_",
                MessageBusLoadTopicProcess = "LoadEventProcessTopic",
                MessageBusServerStatisticsTopic = "ServerStatisticsTopic_",
                MessageBusLoadMethodStatisticsTopic = "LoadMethodStatisticsTopic_",
                MessageBusCustomTopic = "CustomEventTopic_",
                MessageBusTopicDataSaveInDays = 365,

                AuthSettingsKey = "1014424c-6826-4729-b4e4-9d0b826bf3f6",
                AuthSettingsIssuer = "https://token.issuer.example.com",
                AuthSettingsExpiryInMinutes = 30,
                AuthSettingsAudience = "https://api.example.com",
                AuthSettingsRefreshExpiryInDays = 10,
                AuthSettingsPublicKey = "-----BEGIN PUBLIC KEY----- MFswDQYJKoZIhvcNAQEBBQADSgAwRwJAXTpu22oVTZe7L2OLgOvmuoug3xvJvTj6 1tGEHxaVTr3Yr7jd+K893xON4WNt0fs0RUg++wX/oGm7rZxTrzSP7QIDAQAB -----END PUBLIC KEY-----",
                AuthSettingsPrivateKey = "-----BEGIN RSA PRIVATE KEY----- MIIBOAIBAAJAXTpu22oVTZe7L2OLgOvmuoug3xvJvTj61tGEHxaVTr3Yr7jd+K89 3xON4WNt0fs0RUg++wX/oGm7rZxTrzSP7QIDAQABAkArXC8NK9TGpuhvjPvoNU+1 uZVTz2fP8z1vRjkOAIHnAgN47ZPN47xIV3G6zh6eQ8Z2KLkT8iLZrgbQ5Uv85X1B AiEAoaCg538XGbgrUXXSF+dq+R5LbCc7oibApLOVIvMTVP0CIQCTqcnS4sh9rmNz zq6GnfBhorT4jGzW1m7svYo17poRsQIgBb8KOXDBR37Ou3Su5X6qxPSYxd2XNyqd ir1/veBafZkCIDI6CqBk63V0n/eAUfUQO/e59Hymp07cWZbFUgHOSzHBAiB22+OZ umPlOWUAnBulsEiiCNkUQovNZHqDevvClm4AIQ== -----END RSA PRIVATE KEY-----",

                EmailServiceType = "AWS",
                EmailConnectionString = "",
                EmailSenderAddress = "",
                EmailAccessKey = "",
                EmailSecretKey = "",
                EmailRegion = "",

                PostgresDb = "serverpulsedb",
                PostgresUser = "user1",
                PostgresPassword = "ZnwtSxh6DQqR",

                RedisPassword = "ZnwtSxh6DQqR",

                CacheGetServerSlotByEmailExpiryInSeconds = 1,
                CacheServerSlotCheckExpiryInSeconds = 3,
                CacheGetLoadEventsInDataRangeExpiryInMinutes = 0.1,
                CacheGetDailyLoadAmountStatisticsExpiryInMinutes = 0.1,
                CacheGetLoadAmountStatisticsInRangeExpiryInMinutes = 0.1,
                CacheGetSlotStatisticsExpiryInMinutes = 1,

                ServerSlotApiCheck = "/serverslot/check",

                LoadEventProcessingBatchSize = 10,
                LoadEventProcessingBatchIntervalInMilliseconds = 10000,

                BackgroundServicesUseUserUnconfirmedCleanup = false,
                BackgroundServicesUnconfirmedUsersCleanUpInMinutes = 60,

                FeatureManagementOAuth = false,
                FeatureManagementEmailConfirmation = false,

                UseCORS = true,
                EFCreateDatabase = true,
                AllowedCORSOrigins = "",

                PulseEventIntervalInMilliseconds = 20000,
                StatisticsCollectIntervalInMilliseconds = 5000,
                MinimumStatisticsTimeSpanInSeconds = 5,
                MaxEventAmountToGetInSlotData = 25,
                MaxEventAmountToReadPerRequest = 50,
                SlotsPerUser = 5
            };

            new ServerPulseCdkStack(app, "ServerPulseStack", props, images);

            app.Synth();
        }
    }
}
