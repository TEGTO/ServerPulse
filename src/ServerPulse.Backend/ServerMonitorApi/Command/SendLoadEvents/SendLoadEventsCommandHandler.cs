using MediatR;
using MessageBus.Interfaces;
using ServerMonitorApi.Services;
using System.Text.Json;

namespace ServerMonitorApi.Command.SendLoadEvents
{
    public class SendLoadEventsCommandHandler : IRequestHandler<SendLoadEventsCommand, Unit>
    {
        private readonly ISlotKeyChecker serverSlotChecker;
        private readonly IMessageProducer producer;
        private readonly IStatisticsEventSender statisticsEventSender;
        private readonly string loadTopic;

        public SendLoadEventsCommandHandler(ISlotKeyChecker serverSlotChecker, IMessageProducer producer, IStatisticsEventSender statisticsEventSender, IConfiguration configuration)
        {
            this.serverSlotChecker = serverSlotChecker;
            this.producer = producer;
            this.statisticsEventSender = statisticsEventSender;

            loadTopic = configuration[Configuration.KAFKA_LOAD_TOPIC]!;
        }

        public async Task<Unit> Handle(SendLoadEventsCommand command, CancellationToken cancellationToken)
        {
            var events = command.Events;

            if (events != null && events.Length > 0)
            {
                var firstKey = events[0].Key;

                if (!Array.TrueForAll(events, x => x.Key == firstKey))
                {
                    throw new InvalidOperationException($"All events must have the same key per request!");
                }

                if (await serverSlotChecker.CheckSlotKeyAsync(firstKey, cancellationToken))
                {
                    var topic = loadTopic + firstKey;

                    await Parallel.ForEachAsync(events, cancellationToken, async (ev, ct) =>
                    {
                        await statisticsEventSender.SendLoadEventForStatistics(ev, cancellationToken);

                        var serializedEvent = JsonSerializer.Serialize(ev);
                        await producer.ProduceAsync(topic, serializedEvent, cancellationToken);
                    });
                }
                else
                {
                    throw new InvalidOperationException($"Server slot with key '{firstKey}' is not found!");
                }

                return Unit.Value;
            }
            else
            {
                throw new InvalidDataException("Event array could not be null or empty!");
            }
        }
    }
}
