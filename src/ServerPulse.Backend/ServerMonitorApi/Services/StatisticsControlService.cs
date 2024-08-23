using MessageBus.Interfaces;

namespace ServerMonitorApi.Services
{
    public class StatisticsControlService : IStatisticsControlService
    {
        private readonly ITopicManager topicManager;
        private readonly IConfiguration configuration;

        public StatisticsControlService(ITopicManager topicManager, IConfiguration configuration)
        {
            this.topicManager = topicManager;
            this.configuration = configuration;
        }

        public async Task DeleteStatisticsByKeyAsync(string key)
        {
            List<string> topics = new List<string>
            {
                configuration[Configuration.KAFKA_CONFIGURATION_TOPIC]! + key,
                configuration[Configuration.KAFKA_ALIVE_TOPIC]! + key,
                configuration[Configuration.KAFKA_LOAD_TOPIC]! + key,
                configuration[Configuration.KAFKA_CUSTOM_TOPIC]! + key,
            };
            await topicManager.DeleteTopicsAsync(topics);
        }
    }
}