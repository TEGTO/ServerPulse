using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Services.Interfaces;
using AutoMapper;
using MessageBus.Interfaces;
using Shared;

namespace AnalyzerApi.Services.Receivers.Statistics
{

    public class StatisticsReceiver<TStatistics> : BaseReceiver, IStatisticsReceiver<TStatistics>
        where TStatistics : BaseStatistics
    {
        protected readonly StatisticsReceiverTopicData<TStatistics> topicData;

        public StatisticsReceiver(IMessageConsumer messageConsumer, IMapper mapper, IConfiguration configuration, StatisticsReceiverTopicData<TStatistics> topicData) : base(messageConsumer, mapper, configuration)
        {
            this.topicData = topicData;
        }

        #region IStatisticsReceiver<TStatistics> Members

        public virtual async Task<TStatistics?> ReceiveLastStatisticsAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetTopic(topicData.topicOriginName, key);
            var response = await ReceiveLastMessageByKeyAsync(topic, cancellationToken);
            if (response != null)
            {
                response.Message.TryToDeserialize(out TStatistics? statistics);
                return statistics;
            }
            return null;
        }
        public virtual Task<IEnumerable<TStatistics>> GetWholeStatisticsInTimeSpanAsync(string key, TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            return GetLastStatisticsAsEnumerableAsync(key, cancellationToken);
        }
        public virtual Task<IEnumerable<TStatistics>> GetStatisticsInRangeAsync(InRangeQueryOptions options, TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            return GetLastStatisticsAsEnumerableAsync(options.Key, cancellationToken);
        }

        #endregion

        #region Protected Helpers

        private async Task<IEnumerable<TStatistics>> GetLastStatisticsAsEnumerableAsync(string key, CancellationToken cancellationToken)
        {
            List<TStatistics> statisticsList = new List<TStatistics>();
            string topic = GetTopic(topicData.topicOriginName, key);
            var response = await ReceiveLastMessageByKeyAsync(topic, cancellationToken);
            if (response != null)
            {
                if (response.Message.TryToDeserialize(out TStatistics? statistics))
                {
                    statisticsList.Add(statistics!);
                }
            }
            return statisticsList;
        }

        #endregion
    }
}