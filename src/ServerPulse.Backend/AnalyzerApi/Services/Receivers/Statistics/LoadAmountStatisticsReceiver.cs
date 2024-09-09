using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;
using AutoMapper;
using MessageBus.Interfaces;

namespace AnalyzerApi.Services.Receivers.Statistics
{
    public sealed class LoadAmountStatisticsReceiver : StatisticsReceiver<LoadAmountStatistics>
    {
        private readonly int statisticsSaveDataInDays;

        public LoadAmountStatisticsReceiver(IMessageConsumer messageConsumer, IMapper mapper, IConfiguration configuration, StatisticsReceiverTopicData<LoadAmountStatistics> topicData) : base(messageConsumer, mapper, configuration, topicData)
        {
            statisticsSaveDataInDays = int.Parse(configuration[Configuration.KAFKA_TOPIC_DATA_SAVE_IN_DAYS]!);
        }

        #region Override Members

        public override async Task<LoadAmountStatistics?> ReceiveLastStatisticsAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetTopic(topicData.topicOriginName, key);
            var start = DateTime.UtcNow.Date.AddDays(-1);
            var end = DateTime.UtcNow.Date.AddDays(1);
            var timeSpan = TimeSpan.FromDays(1);
            var options = new MessageInRangeQueryOptions(topic, timeoutInMilliseconds, start, end);
            var messagesPerDay = await messageConsumer.GetMessageAmountPerTimespanAsync(options, timeSpan, cancellationToken);
            return ConvertToAmountStatistics(messagesPerDay, timeSpan).FirstOrDefault();
        }
        public override async Task<IEnumerable<LoadAmountStatistics>> GetWholeStatisticsInTimeSpanAsync(string key, TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            string topic = GetTopic(topicData.topicOriginName, key);
            var start = DateTime.UtcNow.Date.AddDays(-statisticsSaveDataInDays);
            var end = DateTime.UtcNow.Date.AddDays(1);
            var options = new MessageInRangeQueryOptions(topic, timeoutInMilliseconds, start, end);
            var messagesPerDay = await messageConsumer.GetMessageAmountPerTimespanAsync(options, timeSpan, cancellationToken);
            return ConvertToAmountStatistics(messagesPerDay, timeSpan);
        }
        public override async Task<IEnumerable<LoadAmountStatistics>> GetStatisticsInRangeAsync(InRangeQueryOptions options, TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            string topic = GetTopic(topicData.topicOriginName, options.Key);
            var messageOptions = new MessageInRangeQueryOptions(topic, timeoutInMilliseconds, options.From, options.To);
            var messagesPerDay = await messageConsumer.GetMessageAmountPerTimespanAsync(messageOptions, timeSpan, cancellationToken);
            return ConvertToAmountStatistics(messagesPerDay, timeSpan);
        }

        #endregion

        #region Private Helpers

        private IEnumerable<LoadAmountStatistics> ConvertToAmountStatistics(Dictionary<DateTime, int> messageAmount, TimeSpan timeSpan)
        {
            return messageAmount
                .Where(kv => kv.Value > 0)
                .Select(kv => new LoadAmountStatistics
                {
                    AmountOfEvents = kv.Value,
                    DateFrom = kv.Key,
                    DateTo = kv.Key + timeSpan
                })
                .OrderByDescending(ls => ls.DateFrom);
        }

        #endregion
    }
}