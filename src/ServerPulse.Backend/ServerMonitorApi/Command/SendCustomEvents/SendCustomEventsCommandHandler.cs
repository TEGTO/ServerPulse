using MediatR;
using MessageBus.Interfaces;
using Microsoft.Extensions.Options;
using ServerMonitorApi.Options;
using ServerMonitorApi.Services;
using System.Text.Json;

namespace ServerMonitorApi.Command.SendCustomEvents
{
    public class SendCustomEventsCommandHandler : IRequestHandler<SendCustomEventsCommand, Unit>
    {
        private readonly ISlotKeyChecker serverSlotChecker;
        private readonly IMessageProducer producer;
        private readonly string customTopic;

        public SendCustomEventsCommandHandler(ISlotKeyChecker serverSlotChecker, IMessageProducer producer, IOptions<MessageBusSettings> options)
        {
            this.serverSlotChecker = serverSlotChecker;
            this.producer = producer;

            customTopic = options.Value.CustomTopic;
        }

        public async Task<Unit> Handle(SendCustomEventsCommand command, CancellationToken cancellationToken)
        {
            var events = command.Events;

            if (events == null || events.Length == 0)
            {
                throw new InvalidDataException("Event array could not be null or empty!");
            }

            var customEvents = events.Select(x => x.CustomEvent).ToArray();
            var customSerializedEvents = events.Select(x => x.CustomEventSerialized).ToArray();

            var firstKey = customEvents[0].Key;
            if (!Array.TrueForAll(customEvents, x => x.Key == firstKey))
            {
                throw new InvalidOperationException("All events must have the same key per request!");
            }

            if (await serverSlotChecker.CheckSlotKeyAsync(firstKey, cancellationToken))
            {
                var topic = customTopic + firstKey;

                var validatedSerializedEvents = customEvents.Zip(customSerializedEvents, (eventObj, serialized) =>
                {
                    var deserialized = JsonSerializer.Deserialize<Dictionary<string, object>>(serialized);

                    if (deserialized == null)
                    {
                        throw new InvalidOperationException("Serialized event could not be deserialized!");
                    }

                    deserialized["Key"] = eventObj.Key;
                    deserialized["Name"] = eventObj.Name;
                    deserialized["Id"] = eventObj.Id;
                    deserialized["Description"] = eventObj.Description;

                    return JsonSerializer.Serialize(deserialized);
                }).ToArray();

                await Parallel.ForEachAsync(validatedSerializedEvents, cancellationToken, async (ev, ct) =>
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
    }
}
