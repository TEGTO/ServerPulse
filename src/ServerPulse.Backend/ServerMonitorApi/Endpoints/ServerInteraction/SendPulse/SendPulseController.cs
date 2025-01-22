using EventCommunication;
using ExceptionHandling;
using MessageBus.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ServerMonitorApi.Infrastructure.Services;
using ServerMonitorApi.Settings;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json;

namespace ServerMonitorApi.Endpoints.ServerInteraction.SendPulse
{
    [Route("serverinteraction")]
    [ApiController]
    public class SendPulseController : ControllerBase
    {
        private readonly ISlotKeyChecker serverSlotChecker;
        private readonly IMessageProducer producer;
        private readonly string pulseTopic;

        public SendPulseController(ISlotKeyChecker serverSlotChecker, IMessageProducer producer, IOptions<MessageBusSettings> options)
        {
            this.serverSlotChecker = serverSlotChecker;
            this.producer = producer;
            pulseTopic = options.Value.AliveTopic;
        }

        [HttpPost("pulse")]
        [SwaggerOperation(
            Summary = "Send a pulse event.",
            Description = "Validates and sends pulse event from the user to the system."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendPulse(PulseEvent ev, CancellationToken cancellationToken)
        {
            if (await serverSlotChecker.CheckSlotKeyAsync(ev.Key, cancellationToken))
            {
                var topic = pulseTopic + ev.Key;
                var message = JsonSerializer.Serialize(ev);
                await producer.ProduceAsync(topic, message, cancellationToken);
                return Ok();
            }
            return BadRequest($"Server slot with key '{ev.Key}' is not found!");
        }
    }
}
