using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;
using AutoMapper;
using Confluent.Kafka;
using MessageBus.Interfaces;
using ServerPulse.EventCommunication.Events;
using System.Runtime.CompilerServices;

namespace AnalyzerApi.Services
{
    public record InRangeQueryOptions(string Key, DateTime From, DateTime To);
    public record ReadCertainMessageNumberOptions(string Key, int NumberOfMessages, DateTime StartDate, bool ReadNew);

    public class ServerLoadReceiver : IServerLoadReceiver
    {
        private readonly IMessageConsumer messageConsumer;
        private readonly IMapper mapper;
        private readonly ILogger<ServerLoadReceiver> logger;
        private readonly string loadTopic;
        private readonly int timeoutInMilliseconds;
        private readonly int statisticsSaveDataInDays;

        public ServerLoadReceiver(IMessageConsumer messageConsumer, IMapper mapper, ILogger<ServerLoadReceiver> logger, IConfiguration configuration)
        {
            this.messageConsumer = messageConsumer;
            this.mapper = mapper;
            this.logger = logger;
            loadTopic = configuration[Configuration.KAFKA_LOAD_TOPIC]!;
            timeoutInMilliseconds = int.Parse(configuration[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS]!);
            statisticsSaveDataInDays = int.Parse(configuration[Configuration.KAFKA_TOPIC_DATA_SAVE_IN_DAYS]!);
        }

        public async IAsyncEnumerable<LoadEventWrapper> ConsumeLoadEventAsync(string key, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            string topic = GetLoadTopic(key);

            await foreach (var response in messageConsumer.ConsumeAsync(topic, timeoutInMilliseconds, Offset.End, cancellationToken))
            {
                if (response.TryDeserializeEventWrapper<LoadEvent, LoadEventWrapper>(mapper, out LoadEventWrapper ev))
                {
                    yield return ev;
                }
            }
        }
        public async Task<IEnumerable<LoadEventWrapper>> ReceiveEventsInRangeAsync(InRangeQueryOptions options, CancellationToken cancellationToken)
        {
            string topic = GetLoadTopic(options.Key);
            var messageOptions = new MessageInRangeQueryOptions(topic, timeoutInMilliseconds, options.From, options.To);
            List<ConsumeResponse> responses = await messageConsumer.ReadMessagesInDateRangeAsync(messageOptions, cancellationToken);
            return ConvertToLoadEventWrappers(responses);
        }
        public async Task<IEnumerable<LoadEventWrapper>> GetCertainAmountOfEvents(ReadCertainMessageNumberOptions options, CancellationToken cancellationToken)
        {
            string topic = GetLoadTopic(options.Key);
            var messageOptions = new ReadSomeMessagesOptions(topic, timeoutInMilliseconds, options.NumberOfMessages, options.StartDate, options.ReadNew);
            List<ConsumeResponse> responses = await messageConsumer.ReadSomeMessagesAsync(messageOptions, cancellationToken);
            return ConvertToLoadEventWrappers(responses);
        }
        public async Task<LoadEventWrapper?> ReceiveLastLoadEventByKeyAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetLoadTopic(key);
            return await TaskGetLastEventFromTopic<LoadEvent, LoadEventWrapper>(topic, cancellationToken);
        }
        private async Task<Y?> TaskGetLastEventFromTopic<T, Y>(string topic, CancellationToken cancellationToken) where T : BaseEvent where Y : BaseEventWrapper
        {
            ConsumeResponse? response = await messageConsumer.ReadLastTopicMessageAsync(topic, timeoutInMilliseconds, cancellationToken);
            if (response != null)
            {
                if (response.TryDeserializeEventWrapper<T, Y>(mapper, out Y ev))
                {
                    return ev;
                }
            }
            return null;
        }
        public async Task<int> ReceiveLoadEventAmountByKeyAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetLoadTopic(key);
            int amount = await messageConsumer.GetAmountTopicMessagesAsync(topic, timeoutInMilliseconds, cancellationToken);
            return amount;
        }
        public async Task<IEnumerable<LoadAmountStatistics>> GetAmountStatisticsInDaysAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetLoadTopic(key);
            var start = DateTime.UtcNow.Date.AddDays(-1 * statisticsSaveDataInDays);
            var end = DateTime.UtcNow.Date.AddDays(1);
            var timeSpan = TimeSpan.FromDays(1);
            var options = new MessageInRangeQueryOptions(topic, timeoutInMilliseconds, start, end);
            var messagesPerDay = await messageConsumer.GetMessageAmountPerTimespanAsync(options, timeSpan, cancellationToken);
            return ConvertToAmountStatistics(messagesPerDay);
        }
        public async Task<IEnumerable<LoadAmountStatistics>> GetAmountStatisticsLastDayAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetLoadTopic(key);
            var todayStart = DateTime.UtcNow.Date;
            var todayEnd = DateTime.UtcNow.Date.AddDays(1);
            var timeSpan = TimeSpan.FromDays(1);
            var options = new MessageInRangeQueryOptions(topic, timeoutInMilliseconds, todayStart, todayEnd);
            var messagesPerDay = await messageConsumer.GetMessageAmountPerTimespanAsync(options, timeSpan, cancellationToken);
            return ConvertToAmountStatistics(messagesPerDay);
        }
        public async Task<IEnumerable<LoadAmountStatistics>> GetAmountStatisticsInRangeAsync(InRangeQueryOptions options, TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            string topic = GetLoadTopic(options.Key);
            var messageOptions = new MessageInRangeQueryOptions(topic, timeoutInMilliseconds, options.From, options.To);
            var messagesPerDay = await messageConsumer.GetMessageAmountPerTimespanAsync(messageOptions, timeSpan, cancellationToken);
            return ConvertToAmountStatistics(messagesPerDay);
        }
        private IEnumerable<LoadAmountStatistics> ConvertToAmountStatistics(Dictionary<DateTime, int> messageAmount)
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
        private IEnumerable<LoadEventWrapper> ConvertToLoadEventWrappers(List<ConsumeResponse> reponses)
        {
            List<LoadEventWrapper> events = new List<LoadEventWrapper>();
            foreach (var response in reponses)
            {
                if (response.TryDeserializeEventWrapper<LoadEvent, LoadEventWrapper>(mapper, out LoadEventWrapper ev))
                {
                    events.Add(ev);
                }
            }
            return events;
        }
        private string GetLoadTopic(string key)
        {
            return loadTopic + key;
        }
    }
}