using EventCommunication;
using MessageBus.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ServerMonitorApi.Options;
using ServerMonitorApi.Services;
using System.Text.Json;

namespace ServerMonitorApi.Endpoints.ServerInteraction.SendCustomEvents
{
    [Route("serverinteraction")]
    [ApiController]
    public class SendCustomEventsController : ControllerBase
    {
        private readonly ISlotKeyChecker serverSlotChecker;
        private readonly IMessageProducer producer;
        private readonly string customTopic;

        public SendCustomEventsController(ISlotKeyChecker serverSlotChecker, IMessageProducer producer, IOptions<MessageBusSettings> options)
        {
            this.serverSlotChecker = serverSlotChecker;
            this.producer = producer;

            customTopic = options.Value.CustomTopic;
        }

        [HttpPost("custom")]
        public async Task<IActionResult> SendCustomEvents(CustomEventContainer[] wrappers, CancellationToken cancellationToken)
        {
            if (wrappers == null || wrappers.Length == 0)
            {
                return BadRequest("Event array could not be null or empty!");
            }

            var customEvents = wrappers.Select(x => x.CustomEvent).ToArray();
            var customSerializedEvents = wrappers.Select(x => x.CustomEventSerialized).ToArray();

            var firstKey = customEvents[0].Key;
            if (!Array.TrueForAll(customEvents, x => x.Key == firstKey))
            {
                return BadRequest("All events must have the same key per request!");
            }

            if (await serverSlotChecker.CheckSlotKeyAsync(firstKey, cancellationToken))
            {
                var topic = customTopic + firstKey;

                try
                {
                    var validatedSerializedEvents = customEvents.Zip(customSerializedEvents, (eventObj, serialized) =>
                    {
                        var deserialized = JsonSerializer.Deserialize<Dictionary<string, object>>(serialized);

                        if (deserialized == null)
                        {
                            throw new InvalidOperationException();
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
                catch (Exception)
                {
                    return Conflict("Serialized event could not be deserialized!");
                }
            }
            else
            {
                return BadRequest($"Server slot with key '{firstKey}' is not found!");
            }

            return Ok();
        }
    }
}
