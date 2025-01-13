using AnalyzerApi.Application.Configuration;
using AnalyzerApi.Application.TopicMapping;
using AnalyzerApi.Core.Models.Statistics;
using MessageBus.Interfaces;
using Microsoft.Extensions.Options;
using Shared;

namespace AnalyzerApi.Application.Services.Receivers.Statistics
{
    internal class StatisticsReceiver<TStatistics> : BaseReceiver, IStatisticsReceiver<TStatistics> where TStatistics : BaseStatistics
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