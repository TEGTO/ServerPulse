using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;
using Confluent.Kafka;
using MessageBus.Interfaces;
using MessageBus.Kafka;
using ServerPulse.EventCommunication.Events;
using Shared;
using System.Runtime.CompilerServices;

namespace AnalyzerApi.Services
{
    public record InRangeQueryOptions(string Key, DateTime From, DateTime To);

    public class ServerLoadReceiver : IServerLoadReceiver
    {
        private readonly IMessageConsumer messageConsumer;
        private readonly ILogger<ServerLoadReceiver> logger;
        private readonly string loadTopic;
        private readonly int timeoutInMilliseconds;
        private readonly int statisticsSaveDataInDays;

        public ServerLoadReceiver(IMessageConsumer messageConsumer, ILogger<ServerLoadReceiver> logger, IConfiguration configuration)
        {
            this.messageConsumer = messageConsumer;
            loadTopic = configuration[Configuration.KAFKA_LOAD_TOPIC]!;
            timeoutInMilliseconds = int.Parse(configuration[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS]!);
            statisticsSaveDataInDays = int.Parse(configuration[Configuration.KAFKA_TOPIC_DATA_SAVE_IN_DAYS]!);
            this.logger = logger;
        }

        public async IAsyncEnumerable<LoadEvent> ConsumeLoadEventAsync(string key, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            string topic = GetLoadTopic(key);

            await foreach (var message in messageConsumer.ConsumeAsync(topic, timeoutInMilliseconds, Offset.End, cancellationToken))
            {
                if (message.TryToDeserialize(out LoadEvent loadEvent))
                {
                    yield return loadEvent;
                }
            }
        }
        public async Task<IEnumerable<LoadEvent>> ReceiveEventsInRangeAsync(InRangeQueryOptions options, CancellationToken cancellationToken)
        {
            string topic = GetLoadTopic(options.Key);
            var messageOptions = new MessageInRangeQueryOptions(topic, timeoutInMilliseconds, options.From, options.To);
            List<string> events = await messageConsumer.ReadMessagesInDateRangeAsync(messageOptions, cancellationToken);
            List<LoadEvent> loadEvents = new List<LoadEvent>();
            foreach (var eventString in events)
            {
                if (eventString.TryToDeserialize(out LoadEvent loadEvent))
                {
                    loadEvents.Add(loadEvent);
                }
            }
            return loadEvents;
        }
        public async Task<LoadEvent?> ReceiveLastLoadEventByKeyAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetLoadTopic(key);
            return await TaskGetLastEventFromTopic<LoadEvent>(topic, cancellationToken);
        }
        private async Task<T?> TaskGetLastEventFromTopic<T>(string topic, CancellationToken cancellationToken) where T : BaseEvent
        {
            string? message = await messageConsumer.ReadLastTopicMessageAsync(topic, timeoutInMilliseconds, cancellationToken);
            if (!string.IsNullOrEmpty(message))
            {
                if (message.TryToDeserialize(out T? ev))
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
            var todayStart = DateTime.UtcNow.Date.AddYears(statisticsSaveDataInDays);
            var todayEnd = DateTime.UtcNow.Date.AddDays(1);
            var timeSpan = TimeSpan.FromDays(1);
            var options = new MessageInRangeQueryOptions(topic, timeoutInMilliseconds, todayStart, todayEnd);
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
        private string GetLoadTopic(string key)
        {
            return loadTopic + key;
        }
    }
}