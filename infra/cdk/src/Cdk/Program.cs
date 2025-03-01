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
            };

            var props = new ServerPulseStackProps
            {
                AspCoreEnvironment = "Development",
                KafkaBootstrapServers = "localhost:9092",
                KafkaClientIdAnalyzer = "analyzer",
                KafkaClientIdMonitorApi = "monitorapi",
                KafkaGroupId = "serverpulse",
                KafkaPartitionsAmount = 1,
                MessageBusReceiveTimeoutInMilliseconds = 1000,
                MessageBusAliveTopic = "alive",
                MessageBusConfigurationTopic = "configuration",
                MessageBusLoadTopic = "load",
                MessageBusLoadTopicProcess = "loadprocess",
                MessageBusServerStatisticsTopic = "serverstatistics",
                MessageBusLoadMethodStatisticsTopic = "loadmethodstatistics",
                MessageBusCustomTopic = "custom",
                MessageBusTopicDataSaveInDays = 7,
                AuthSettingsKey = "supersecretkey",
                AuthSettingsIssuer = "serverpulse",
                AuthSettingsExpiryInMinutes = 60,
                AuthSettingsAudience = "serverpulse",
                AuthSettingsRefreshExpiryInDays = 7,
                AuthSettingsPublicKey = "publickey",
                AuthSettingsPrivateKey = "privatekey",
                EmailConnectionString = "emailconnectionstring",
                EmailSenderAddress = "sender",
                EmailAccessKey = "accesskey",
                EmailSecretKey = "secretkey",
                EmailRegion = "region",
                PostgresDb = "postgresdb",
                PostgresUser = "postgresuser",
                PostgresPassword = "postgrespassword",
                RedisPassword = "redispassword",
                ConnectionStringsAuthenticationDb = "authenticationdb",
                ConnectionStringsServerSlotDb = "serverslotdb",
                ConnectionStringsRedisServer = "redisserver",
                CacheGetServerSlotByEmailExpiryInSeconds = 60,
                CacheServerSlotCheckExpiryInSeconds = 60,
                CacheServerSlotExpiryInMinutes = 60,
                CacheGetLoadEventsInDataRangeExpiryInMinutes = 60,
                CacheGetDailyLoadAmountStatisticsExpiryInMinutes = 60,
                CacheGetLoadAmountStatisticsInRangeExpiryInMinutes = 60,
                CacheGetSlotStatisticsExpiryInMinutes = 60,
                ApiGateway = "apigateway",
                ServerSlotApiUrl = "serverslotapiurl",
                ServerSlotApiCheck = "serverslotapicheck",
            };

            new ServerPulseCdkStack(app, "ServerPulseStack", props, images);

            app.Synth();
        }
    }
}
