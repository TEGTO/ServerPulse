using MediatR;
using MessageBus.Interfaces;
using ServerMonitorApi.Services;
using System.Text.Json;

namespace ServerMonitorApi.Command.SendPulse
{
    public class SendPulseCommandHandler : IRequestHandler<SendPulseCommand, Unit>
    {
        private readonly ISlotKeyChecker serverSlotChecker;
        private readonly IMessageProducer producer;
        private readonly string pulseTopic;

        public SendPulseCommandHandler(ISlotKeyChecker serverSlotChecker, IMessageProducer producer, IConfiguration configuration)
        {
            this.serverSlotChecker = serverSlotChecker;
            this.producer = producer;
            pulseTopic = configuration[Configuration.KAFKA_ALIVE_TOPIC]!;
        }

        public async Task<Unit> Handle(SendPulseCommand command, CancellationToken cancellationToken)
        {
            var ev = command.Event;

            if (await serverSlotChecker.CheckSlotKeyAsync(ev.Key, cancellationToken))
            {
                var topic = pulseTopic + ev.Key;

                var message = JsonSerializer.Serialize(ev);

                await producer.ProduceAsync(topic, message, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException($"Server slot with key '{ev.Key}' is not found!");
            }

            return Unit.Value;
        }
    }
}
