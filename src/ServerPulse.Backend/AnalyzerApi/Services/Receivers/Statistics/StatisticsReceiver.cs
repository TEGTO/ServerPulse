using AnalyzerApi.Infrastructure.Configuration;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Infrastructure.TopicMapping;
using MessageBus.Interfaces;
using Microsoft.Extensions.Options;
using Shared;

namespace AnalyzerApi.Services.Receivers.Statistics
{
    public class StatisticsReceiver<TStatistics> : BaseReceiver, IStatisticsReceiver<TStatistics> where TStatistics : BaseStatistics
    {
        protected readonly StatisticsTopicMapping<TStatistics> topicData;

        public StatisticsReceiver(IMessageConsumer messageConsumer, IOptions<MessageBusSettings> options, StatisticsTopicMapping<TStatistics> topicData) : base(messageConsumer, options)
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