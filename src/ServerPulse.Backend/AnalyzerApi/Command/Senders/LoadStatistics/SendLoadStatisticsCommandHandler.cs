using AnalyzerApi.Hubs;
using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace AnalyzerApi.Command.Senders.LoadStatistics
{
    public class SendLoadStatisticsCommandHandler : IRequestHandler<SendLoadStatisticsCommand, Unit>
    {
        private readonly IMapper mapper;
        private readonly IHubContext<StatisticsHub<ServerLoadStatistics>, IStatisticsHubClient> hubStatistics;

        public SendLoadStatisticsCommandHandler(IHubContext<StatisticsHub<ServerLoadStatistics>, IStatisticsHubClient> hubStatistics, IMapper mapper)
        {
            this.hubStatistics = hubStatistics;
            this.mapper = mapper;
        }

        public async Task<Unit> Handle(SendLoadStatisticsCommand command, CancellationToken cancellationToken)
        {
            var response = mapper.Map<ServerLoadStatisticsResponse>(command.Statistics);
            var serializedData = JsonSerializer.Serialize(response);
            await hubStatistics.Clients.Group(command.Key).ReceiveStatistics(command.Key, serializedData);

            return Unit.Value;
        }
    }
}
