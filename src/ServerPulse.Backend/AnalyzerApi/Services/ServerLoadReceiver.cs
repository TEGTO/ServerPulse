using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;
using AutoMapper;
using MessageBus.Interfaces;
using ServerPulse.EventCommunication.Events;
using Shared;

namespace AnalyzerApi.Services
{
    public class ServerLoadReceiver : BaseEventReceiver, IServerLoadReceiver
    {
        private readonly string loadTopic;
        private readonly string loadMethodStatisticsTopic;
        private readonly int statisticsSaveDataInDays;

        public ServerLoadReceiver(IMessageConsumer messageConsumer, IMapper mapper, IConfiguration configuration)
            : base(messageConsumer, mapper, configuration)
        {
            loadTopic = configuration[Configuration.KAFKA_LOAD_TOPIC]!;
            statisticsSaveDataInDays = int.Parse(configuration[Configuration.KAFKA_TOPIC_DATA_SAVE_IN_DAYS]!);
            loadMethodStatisticsTopic = configuration[Configuration.KAFKA_LOAD_METHOD_STATISTICS_TOPIC]!;
        }

        public IAsyncEnumerable<LoadEventWrapper> ConsumeLoadEventAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetTopic(loadTopic, key);
            return ConsumeEventAsync<LoadEvent, LoadEventWrapper>(topic, cancellationToken);
        }

        public async Task<IEnumerable<LoadEventWrapper>> ReceiveEventsInRangeAsync(InRangeQueryOptions options, CancellationToken cancellationToken)
        {
            string topic = GetTopic(loadTopic, options.Key);
            var messageOptions = new MessageInRangeQueryOptions(topic, timeoutInMilliseconds, options.From, options.To);
            List<ConsumeResponse> responses = await messageConsumer.ReadMessagesInDateRangeAsync(messageOptions, cancellationToken);
            return LoadEventHelper.ConvertToLoadEventWrappers(responses, mapper);
        }

        public async Task<IEnumerable<LoadEventWrapper>> GetCertainAmountOfEvents(ReadCertainMessageNumberOptions options, CancellationToken cancellationToken)
        {
            string topic = GetTopic(loadTopic, options.Key);
            var messageOptions = new ReadSomeMessagesOptions(topic, timeoutInMilliseconds, options.NumberOfMessages, options.StartDate, options.ReadNew);
            List<ConsumeResponse> responses = await messageConsumer.ReadSomeMessagesAsync(messageOptions, cancellationToken);
            return LoadEventHelper.ConvertToLoadEventWrappers(responses, mapper);
        }

        public Task<LoadEventWrapper?> ReceiveLastLoadEventByKeyAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetTopic(loadTopic, key);
            return ReceiveLastEventByKeyAsync<LoadEvent, LoadEventWrapper>(topic, cancellationToken);
        }

        public async Task<int> ReceiveLoadEventAmountByKeyAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetTopic(loadTopic, key);
            return await messageConsumer.GetAmountTopicMessagesAsync(topic, timeoutInMilliseconds, cancellationToken);
        }

        public async Task<IEnumerable<LoadAmountStatistics>> GetAmountStatisticsInDaysAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetTopic(loadTopic, key);
            var start = DateTime.UtcNow.Date.AddDays(-statisticsSaveDataInDays);
            var end = DateTime.UtcNow.Date.AddDays(1);
            var timeSpan = TimeSpan.FromDays(1);
            var options = new MessageInRangeQueryOptions(topic, timeoutInMilliseconds, start, end);
            var messagesPerDay = await messageConsumer.GetMessageAmountPerTimespanAsync(options, timeSpan, cancellationToken);
            return LoadEventHelper.ConvertToAmountStatistics(messagesPerDay);
        }

        public async Task<IEnumerable<LoadAmountStatistics>> GetAmountStatisticsLastDayAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetTopic(loadTopic, key);
            var todayStart = DateTime.UtcNow.Date;
            var todayEnd = DateTime.UtcNow.Date.AddDays(1);
            var timeSpan = TimeSpan.FromDays(1);
            var options = new MessageInRangeQueryOptions(topic, timeoutInMilliseconds, todayStart, todayEnd);
            var messagesPerDay = await messageConsumer.GetMessageAmountPerTimespanAsync(options, timeSpan, cancellationToken);
            return LoadEventHelper.ConvertToAmountStatistics(messagesPerDay);
        }

        public async Task<IEnumerable<LoadAmountStatistics>> GetAmountStatisticsInRangeAsync(InRangeQueryOptions options, TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            string topic = GetTopic(loadTopic, options.Key);
            var messageOptions = new MessageInRangeQueryOptions(topic, timeoutInMilliseconds, options.From, options.To);
            var messagesPerDay = await messageConsumer.GetMessageAmountPerTimespanAsync(messageOptions, timeSpan, cancellationToken);
            return LoadEventHelper.ConvertToAmountStatistics(messagesPerDay);
        }

        public async Task<LoadMethodStatistics?> ReceiveLastLoadMethodStatisticsByKeyAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetTopic(loadMethodStatisticsTopic, key);
            var response = await ReceiveLastMessageByKeyAsync(topic, cancellationToken);
            response.TryToDeserialize(out LoadMethodStatistics? statistics);
            return statistics;
        }
    }

    internal static class LoadEventHelper
    {
        public static IEnumerable<LoadAmountStatistics> ConvertToAmountStatistics(Dictionary<DateTime, int> messageAmount)
        {
            return messageAmount
                .Where(kv => kv.Value > 0)
                .Select(kv => new LoadAmountStatistics
                {
                    AmountOfEvents = kv.Value,
                    Date = kv.Key
                })
                .OrderByDescending(ls => ls.Date);
        }

        public static IEnumerable<LoadEventWrapper> ConvertToLoadEventWrappers(List<ConsumeResponse> responses, IMapper mapper)
        {
            List<LoadEventWrapper> events = new List<LoadEventWrapper>();
            foreach (var response in responses)
            {
                if (response.TryDeserializeEventWrapper<LoadEvent, LoadEventWrapper>(mapper, out LoadEventWrapper ev))
                {
                    events.Add(ev);
                }
            }
            return events;
        }
    }
}