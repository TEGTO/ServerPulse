using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;
using MessageBus.Interfaces;
using ServerPulse.EventCommunication.Events;
using System.Text.Json;

namespace AnalyzerApi.Services
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IMessageProducer producer;
        private readonly IServerLoadReceiver serverLoadReceiver;
        private readonly string loadMethodStatisticsTopic;

        public EventProcessor(IMessageProducer producer, IServerLoadReceiver serverLoadReceiver, IConfiguration configuration)
        {
            this.producer = producer;
            this.serverLoadReceiver = serverLoadReceiver;
            loadMethodStatisticsTopic = configuration[Configuration.KAFKA_LOAD_METHOD_STATISTICS_TOPIC]!;
        }

        #region IEventProcessor Members

        public Task ProcessEventsAsync<T>(T[] events, CancellationToken cancellationToken) where T : BaseEvent
        {
            if (events == null || events.Length == 0)
            {
                throw new InvalidDataException("Invalid events!");
            }

            if (!events.All(x => x.Key == events.First().Key))
            {
                throw new InvalidDataException("All events must have the same key per request!");
            }

            return events.First() switch
            {
                LoadEvent _ => ProcessLoadEventsAsync((events as LoadEvent[])!, cancellationToken),
                _ => Task.CompletedTask
            };
        }

        #endregion

        #region Private Members

        private async Task ProcessLoadEventsAsync(LoadEvent[] events, CancellationToken cancellationToken)
        {
            var firstKey = events.First().Key;
            var statistics = await serverLoadReceiver.ReceiveLastLoadMethodStatisticsByKeyAsync(firstKey, cancellationToken);

            if (statistics == null)
            {
                statistics = new LoadMethodStatistics();
            }

            foreach (var loadEvent in events)
            {
                AddMethodToStatistics(loadEvent.Method, statistics);
            }

            string topic = GetLoadMethodStatisticsTopic(firstKey);
            await producer.ProduceAsync(topic, JsonSerializer.Serialize(statistics), cancellationToken);
        }
        private void AddMethodToStatistics(string method, LoadMethodStatistics statistics)
        {
            var methodCounters = new Dictionary<string, Action>
            {
                { "GET", () => statistics.GetAmount++ },
                { "POST", () => statistics.PostAmount++ },
                { "PUT", () => statistics.PutAmount++ },
                { "DELETE", () => statistics.DeleteAmount++ },
                { "PATCH", () => statistics.PatchAmount++ }
            };

            if (methodCounters.TryGetValue(method.ToUpper(), out var incrementMethod))
            {
                incrementMethod();
            }
        }
        private string GetLoadMethodStatisticsTopic(string key)
        {
            return loadMethodStatisticsTopic + key;
        }

        #endregion
    }
}