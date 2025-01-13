using EventCommunication;
using MessageBus.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ServerMonitorApi.Infrastructure.Services;
using ServerMonitorApi.Settings;
using System.Text.Json;

namespace ServerMonitorApi.Endpoints.ServerInteraction.SendLoadEvents
{
    [Route("serverinteraction")]
    [ApiController]
    public class SendLoadEventsController : ControllerBase
    {
        private readonly ISlotKeyChecker serverSlotChecker;
        private readonly IMessageProducer producer;
        private readonly string loadTopic;
        private readonly string processLoadEventTopic;

        public SendLoadEventsController(ISlotKeyChecker serverSlotChecker, IMessageProducer producer, IOptions<MessageBusSettings> options)
        {
            this.serverSlotChecker = serverSlotChecker;
            this.producer = producer;

            loadTopic = options.Value.LoadTopic;
            processLoadEventTopic = options.Value.LoadTopicProcess;
        }

        [HttpPost("load")]
        public async Task<IActionResult> SendLoadEvents(LoadEvent[] events, CancellationToken cancellationToken)
        {
            if (events != null && events.Length > 0)
            {
                var firstKey = events[0].Key;

                if (!Array.TrueForAll(events, x => x.Key == firstKey))
                {
                    return BadRequest($"All events must have the same key per request!");
                }

                if (await serverSlotChecker.CheckSlotKeyAsync(firstKey, cancellationToken))
                {
                    var topic = loadTopic + firstKey;

                    await Parallel.ForEachAsync(events, cancellationToken, async (ev, ct) =>
                    {
                        var serializedEvent = JsonSerializer.Serialize(ev);
                        await producer.ProduceAsync(topic, serializedEvent, cancellationToken);
                        await producer.ProduceAsync(processLoadEventTopic, serializedEvent, cancellationToken);
                    });

                    return Ok();
                }
                else
                {
                    return BadRequest($"Server slot with key '{firstKey}' is not found!");
                }
            }
            else
            {
                return BadRequest("Event array could not be null or empty!");
            }
        }
    }
}
