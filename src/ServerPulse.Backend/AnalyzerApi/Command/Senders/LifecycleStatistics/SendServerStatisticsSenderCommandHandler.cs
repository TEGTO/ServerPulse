using AnalyzerApi.Hubs;
using AnalyzerApi.Infrastructure;
using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AutoMapper;
using MediatR;
using MessageBus.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace AnalyzerApi.Command.Senders.LifecycleStatistics
{
    public class SendServerStatisticsSenderCommandHandler : IRequestHandler<SendServerStatisticsSenderCommand, Unit>
    {
        private readonly IMessageProducer producer;
        private readonly IHubContext<StatisticsHub<ServerLifecycleStatistics>, IStatisticsHubClient> hubStatistics;
        private readonly IMapper mapper;
        private readonly string serverStatisticsTopic;

        public SendServerStatisticsSenderCommandHandler(IMessageProducer producer, IHubContext<StatisticsHub<ServerLifecycleStatistics>, IStatisticsHubClient> hubStatistics, IMapper mapper, IConfiguration configuration)
        {
            this.producer = producer;
            this.hubStatistics = hubStatistics;
            this.mapper = mapper;
            serverStatisticsTopic = configuration[Configuration.KAFKA_SERVER_STATISTICS_TOPIC]!;
        }

        public async Task<Unit> Handle(SendServerStatisticsSenderCommand command, CancellationToken cancellationToken)
        {
            var topic = serverStatisticsTopic + command.Key;

            await producer.ProduceAsync(topic, JsonSerializer.Serialize(command.Statistics), cancellationToken);

            var response = mapper.Map<ServerLifecycleStatisticsResponse>(command.Statistics);
            var serializedData = JsonSerializer.Serialize(response);

            await hubStatistics.Clients.Group(command.Key).ReceiveStatistics(command.Key, serializedData);

            return Unit.Value;
        }
    }
}
