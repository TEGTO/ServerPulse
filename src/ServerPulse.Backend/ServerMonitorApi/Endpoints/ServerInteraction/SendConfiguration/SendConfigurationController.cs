using EventCommunication;
using MessageBus.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ServerMonitorApi.Infrastructure.Services;
using ServerMonitorApi.Settings;
using System.Text.Json;

namespace ServerMonitorApi.Endpoints.ServerInteraction.SendConfiguration
{
    [Route("serverinteraction")]
    [ApiController]
    public class SendConfigurationController : ControllerBase
    {
        private readonly ISlotKeyChecker serverSlotChecker;
        private readonly IMessageProducer producer;
        private readonly string configurationTopic;

        public SendConfigurationController(ISlotKeyChecker serverSlotChecker, IMessageProducer producer, IOptions<MessageBusSettings> options)
        {
            this.serverSlotChecker = serverSlotChecker;
            this.producer = producer;
            configurationTopic = options.Value.ConfigurationTopic;
        }

        [HttpPost("configuration")]
        public async Task<IActionResult> SendConfiguration(ConfigurationEvent ev, CancellationToken cancellationToken)
        {
            if (await serverSlotChecker.CheckSlotKeyAsync(ev.Key, cancellationToken))
            {
                var topic = configurationTopic + ev.Key;

                var message = JsonSerializer.Serialize(ev);

                await producer.ProduceAsync(topic, message, cancellationToken);
                return Ok();
            }
            return BadRequest($"Server slot with key '{ev.Key}' is not found!");
        }
    }
}
