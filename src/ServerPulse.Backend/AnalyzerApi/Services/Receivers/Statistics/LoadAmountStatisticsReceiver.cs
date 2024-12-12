using AnalyzerApi.Infrastructure;
using AnalyzerApi.Infrastructure.Configurations;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AutoMapper;
using MessageBus.Interfaces;

namespace AnalyzerApi.Services.Receivers.Statistics
{
    public sealed class LoadAmountStatisticsReceiver : StatisticsReceiver<LoadAmountStatistics>, ILoadAmountStatisticsReceiver
    {
        private readonly int statisticsSaveDataInDays;

        public LoadAmountStatisticsReceiver(IMessageConsumer messageConsumer, IMapper mapper, IConfiguration configuration, StatisticsReceiverTopicConfiguration<LoadAmountStatistics> topicData) : base(messageConsumer, mapper, configuration, topicData)
        {
            statisticsSaveDataInDays = int.Parse(configuration[Configuration.KAFKA_TOPIC_DATA_SAVE_IN_DAYS]!);
        }

        public override async Task<LoadAmountStatistics?> GetLastStatisticsAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetTopic(topicData.TopicOriginName, key);

            var start = DateTime.UtcNow.Date.AddDays(-1);
            var end = DateTime.UtcNow.Date.AddDays(1);

            var timeSpan = TimeSpan.FromDays(1);

            var options = new MessageInRangeQueryOptions(topic, timeoutInMilliseconds, start, end);

            var messagesPerDay = await messageConsumer.GetMessageAmountPerTimespanAsync(options, timeSpan, cancellationToken);

            return ConvertToAmountStatistics(messagesPerDay, timeSpan).FirstOrDefault();
        }

        public async Task<IEnumerable<LoadAmountStatistics>> GetWholeStatisticsInTimeSpanAsync(string key, TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            string topic = GetTopic(topicData.TopicOriginName, key);

            var start = DateTime.UtcNow.Date.AddDays(-statisticsSaveDataInDays);
            var end = DateTime.UtcNow.Date.AddDays(1);

            var options = new MessageInRangeQueryOptions(topic, timeoutInMilliseconds, start, end);
            var messagesPerDay = await messageConsumer.GetMessageAmountPerTimespanAsync(options, timeSpan, cancellationToken);

            return ConvertToAmountStatistics(messagesPerDay, timeSpan);
        }

        public async Task<IEnumerable<LoadAmountStatistics>> GetStatisticsInRangeAsync(InRangeQuery options, TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            string topic = GetTopic(topicData.TopicOriginName, options.Key);

            var messageOptions = new MessageInRangeQueryOptions(topic, timeoutInMilliseconds, options.From, options.To);
            var messagesPerDay = await messageConsumer.GetMessageAmountPerTimespanAsync(messageOptions, timeSpan, cancellationToken);

            return ConvertToAmountStatistics(messagesPerDay, timeSpan);
        }

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
    }
}