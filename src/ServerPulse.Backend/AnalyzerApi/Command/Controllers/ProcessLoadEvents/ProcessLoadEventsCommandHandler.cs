using AnalyzerApi.Infrastructure;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Services.Receivers.Statistics;
using EventCommunication.Events;
using MediatR;
using MessageBus.Interfaces;
using System.Text.Json;

namespace AnalyzerApi.Command.Controllers.ProcessLoadEvents
{
    public class ProcessLoadEventsCommandHandler : IRequestHandler<ProcessLoadEventsCommand, Unit>
    {
        private readonly IMessageProducer producer;
        private readonly IStatisticsReceiver<LoadMethodStatistics> receiver;
        private readonly string loadMethodStatisticsTopic;

        public ProcessLoadEventsCommandHandler(IMessageProducer producer, IStatisticsReceiver<LoadMethodStatistics> statisticsReceiver, IConfiguration configuration)
        {
            this.producer = producer;
            this.receiver = statisticsReceiver;
            loadMethodStatisticsTopic = configuration[Configuration.KAFKA_LOAD_METHOD_STATISTICS_TOPIC]!;
        }

        public async Task<Unit> Handle(ProcessLoadEventsCommand command, CancellationToken cancellationToken)
        {
            var events = command.Events;

            ValidateCommand(command);

            await ProcessLoadEventsAsync(events, cancellationToken);

            return Unit.Value;
        }

        #region Private Helpers

        private static void ValidateCommand(ProcessLoadEventsCommand command)
        {
            var events = command.Events;

            if (events == null || events.Length == 0)
            {
                throw new InvalidDataException("Events could not be null or empty!");
            }
            else if (!events.All(x => x.Key == events[0].Key))
            {
                throw new InvalidDataException("All event keys must be the same!");
            }
        }

        private async Task ProcessLoadEventsAsync(LoadEvent[] events, CancellationToken cancellationToken)
        {
            var firstKey = events[0].Key;

            var statistics = await receiver.GetLastStatisticsAsync(firstKey, cancellationToken);

            if (statistics == null)
            {
                statistics = new LoadMethodStatistics();
            }

            foreach (var loadEvent in events)
            {
                AddMethodToStatistics(loadEvent.Method, statistics);
            }

            var topic = loadMethodStatisticsTopic + firstKey;

            await producer.ProduceAsync(topic, JsonSerializer.Serialize(statistics), cancellationToken);
        }

        private static void AddMethodToStatistics(string method, LoadMethodStatistics statistics)
        {
            switch (method.ToUpper())
            {
                case "GET":
                    statistics.GetAmount++;
                    break;
                case "POST":
                    statistics.PostAmount++;
                    break;
                case "PUT":
                    statistics.PutAmount++;
                    break;
                case "DELETE":
                    statistics.DeleteAmount++;
                    break;
                case "PATCH":
                    statistics.PatchAmount++;
                    break;
            }
        }

        #endregion
    }
}
