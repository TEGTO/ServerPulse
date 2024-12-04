using MediatR;
using MessageBus.Interfaces;
using ServerMonitorApi.Services;
using Shared.Helpers;
using System.Text.Json;

namespace ServerMonitorApi.Command.SendLoadEvents
{
    public class SendLoadEventsCommandHandler : IRequestHandler<SendLoadEventsCommand, Unit>
    {
        private readonly ISlotKeyChecker serverSlotChecker;
        private readonly IMessageProducer producer;
        private readonly IHttpHelper httpHelper;
        private readonly string loadTopic;
        private readonly string loadAnalyzeUri;

        public SendLoadEventsCommandHandler(ISlotKeyChecker serverSlotChecker, IMessageProducer producer, IHttpHelper httpHelper, IConfiguration configuration)
        {
            this.serverSlotChecker = serverSlotChecker;
            this.producer = producer;
            this.httpHelper = httpHelper;

            loadTopic = configuration[Configuration.KAFKA_LOAD_TOPIC]!;
            loadAnalyzeUri = $"{configuration[Configuration.API_GATEWAY]}{configuration[Configuration.ANALYZER_LOAD_ANALYZE]}";
        }

        public async Task<Unit> Handle(SendLoadEventsCommand command, CancellationToken cancellationToken)
        {
            var events = command.Events;

            if (events != null && events.Length > 0)
            {
                var firstKey = events[0].Key;

                if (!Array.TrueForAll(events, x => x.Key == firstKey))
                {
                    throw new InvalidOperationException($"All load events must have the same key per request!");
                }

                if (await serverSlotChecker.CheckSlotKeyAsync(firstKey, cancellationToken))
                {
                    var topic = loadTopic + firstKey;

                    var message = JsonSerializer.Serialize(events);

                    await SendEventsForStatisticsProcessing(message, cancellationToken);
                    await producer.ProduceAsync(topic, message, cancellationToken);
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

        private async Task SendEventsForStatisticsProcessing(string serializedEvents, CancellationToken cancellationToken)
        {
            await httpHelper.SendPostRequestAsync<dynamic>(loadAnalyzeUri, serializedEvents, null, cancellationToken);
        }
    }
}
