using MediatR;
using MessageBus.Interfaces;
using ServerMonitorApi.Services;
using System.Text.Json;

namespace ServerMonitorApi.Command.SendConfiguration
{
    public class SendConfigurationCommandHandler : IRequestHandler<SendConfigurationCommand, Unit>
    {
        private readonly ISlotKeyChecker serverSlotChecker;
        private readonly IMessageProducer producer;
        private readonly string configurationTopic;

        public SendConfigurationCommandHandler(ISlotKeyChecker serverSlotChecker, IMessageProducer producer, IConfiguration configuration)
        {
            this.serverSlotChecker = serverSlotChecker;
            this.producer = producer;
            configurationTopic = configuration[Configuration.KAFKA_CONFIGURATION_TOPIC]!;
        }

        public async Task<Unit> Handle(SendConfigurationCommand command, CancellationToken cancellationToken)
        {
            var ev = command.Event;

            if (await serverSlotChecker.CheckSlotKeyAsync(ev.Key, cancellationToken))
            {
                var topic = configurationTopic + ev.Key;

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
