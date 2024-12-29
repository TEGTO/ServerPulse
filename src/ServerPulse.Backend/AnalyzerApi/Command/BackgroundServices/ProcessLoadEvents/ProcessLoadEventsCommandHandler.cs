using AnalyzerApi.Infrastructure.Configuration;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Services.Receivers.Statistics;
using EventCommunication;
using MediatR;
using MessageBus.Interfaces;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text.Json;

namespace AnalyzerApi.Command.BackgroundServices.ProcessLoadEvents
{
    public class ProcessLoadEventsCommandHandler : IRequestHandler<ProcessLoadEventsCommand, Unit>
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> keyLocks = new();

        private readonly IMessageProducer producer;
        private readonly IStatisticsReceiver<LoadMethodStatistics> receiver;
        private readonly string loadMethodStatisticsTopic;

        public ProcessLoadEventsCommandHandler(IMessageProducer producer, IStatisticsReceiver<LoadMethodStatistics> receiver, IOptions<MessageBusSettings> options)
        {
            this.producer = producer;
            this.receiver = receiver;
            loadMethodStatisticsTopic = options.Value.LoadMethodStatisticsTopic;
        }

        public async Task<Unit> Handle(ProcessLoadEventsCommand command, CancellationToken cancellationToken)
        {
            var events = command.Events;

            ValidateCommand(events);

            var groupedEvents = events.GroupBy(e => e.Key);

            var tasks = groupedEvents.Select(group => ProcessLoadEventsForKeyWithLockAsync(group.Key, group.ToArray(), cancellationToken));

            await Task.WhenAll(tasks);

            return Unit.Value;
        }

        private static void ValidateCommand(LoadEvent[] events)
        {
            if (events == null || events.Length == 0)
            {
                throw new InvalidDataException("Events could not be null or empty!");
            }
        }

        private async Task ProcessLoadEventsForKeyWithLockAsync(string key, LoadEvent[] events, CancellationToken cancellationToken)
        {
            var semaphore = keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var statistics = await receiver.GetLastStatisticsAsync(key, cancellationToken) ?? new LoadMethodStatistics();

                foreach (var loadEvent in events)
                {
                    AddMethodToStatistics(loadEvent.Method, statistics);
                }

                var topic = loadMethodStatisticsTopic + key;

                await producer.ProduceAsync(topic, JsonSerializer.Serialize(statistics), cancellationToken);
            }
            finally
            {
                semaphore.Release();
                keyLocks.TryRemove(key, out _);
            }
        }

        private static void AddMethodToStatistics(string method, LoadMethodStatistics statistics)
        {
            switch (method.ToUpper())
            {
                case "GET":
                    statistics.GetAmount++;
                    break;
                case "POST":
                    statistics.PostAmount++;
                    break;
                case "PUT":
                    statistics.PutAmount++;
                    break;
                case "DELETE":
                    statistics.DeleteAmount++;
                    break;
                case "PATCH":
                    statistics.PatchAmount++;
                    break;
            }
        }
    }
}
