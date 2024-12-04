using MediatR;
using MessageBus.Interfaces;
using ServerMonitorApi.Services;

namespace ServerMonitorApi.Command.SendCustomEvents
{
    public class SendCustomEventsCommandHandler : IRequestHandler<SendCustomEventsCommand, Unit>
    {
        private readonly ISlotKeyChecker serverSlotChecker;
        private readonly IMessageProducer producer;
        private readonly string configurationTopic;

        public SendCustomEventsCommandHandler(ISlotKeyChecker serverSlotChecker, IMessageProducer producer, IConfiguration configuration)
        {
            this.serverSlotChecker = serverSlotChecker;
            this.producer = producer;

            configurationTopic = configuration[Configuration.KAFKA_CONFIGURATION_TOPIC]!;
        }

        public async Task<Unit> Handle(SendCustomEventsCommand command, CancellationToken cancellationToken)
        {
            var events = command.Events;

            if (events != null && events.Length > 0)
            {
                var customEvents = events.Select(x => x.CustomEvent).ToArray();
                var customSerializedEvents = events.Select(x => x.CustomEventSerialized).ToArray();

                var firstKey = customEvents[0].Key;
                if (!Array.TrueForAll(customEvents, x => x.Key == firstKey))
                {
                    throw new InvalidOperationException($"All events must have the same key per request!");
                }

                if (await serverSlotChecker.CheckSlotKeyAsync(firstKey, cancellationToken))
                {
                    var topic = configurationTopic + firstKey;

                    await Parallel.ForEachAsync(customSerializedEvents, cancellationToken, async (ev, ct) =>
                    {
                        await producer.ProduceAsync(topic, ev, cancellationToken);
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
