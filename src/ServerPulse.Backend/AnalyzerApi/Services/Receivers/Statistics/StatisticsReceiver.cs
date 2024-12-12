using AnalyzerApi.Infrastructure.Configurations;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AutoMapper;
using MessageBus.Interfaces;
using Shared;

namespace AnalyzerApi.Services.Receivers.Statistics
{
    public class StatisticsReceiver<TStatistics> : BaseReceiver, IStatisticsReceiver<TStatistics> where TStatistics : BaseStatistics
    {
        protected readonly StatisticsReceiverTopicConfiguration<TStatistics> topicData;

        public StatisticsReceiver(IMessageConsumer messageConsumer, IMapper mapper, IConfiguration configuration, StatisticsReceiverTopicConfiguration<TStatistics> topicData) : base(messageConsumer, mapper, configuration)
        {
            this.topicData = topicData;
        }

        #region IStatisticsReceiver<TStatistics> Members

        public virtual async Task<TStatistics?> GetLastStatisticsAsync(string key, CancellationToken cancellationToken)
        {
            var topic = GetTopic(topicData.TopicOriginName, key);
            var response = await GetLastMessageByKeyAsync(topic, cancellationToken);

            if (response != null)
            {
                response.Message.TryToDeserialize(out TStatistics? statistics);
                return statistics;
            }

            return null;
        }

        #endregion
    }
}