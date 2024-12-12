using AnalyzerApi.Hubs;
using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace AnalyzerApi.Command.Senders.CustomStatistics
{
    public class SendCustomStatisticsCommandHandler : IRequestHandler<SendCustomStatisticsCommand, Unit>
    {
        private readonly IHubContext<StatisticsHub<ServerCustomStatistics>, IStatisticsHubClient> hubStatistics;
        private readonly IMapper mapper;

        public SendCustomStatisticsCommandHandler(IHubContext<StatisticsHub<ServerCustomStatistics>, IStatisticsHubClient> hubStatistics, IMapper mapper)
        {
            this.hubStatistics = hubStatistics;
            this.mapper = mapper;
        }

        public async Task<Unit> Handle(SendCustomStatisticsCommand command, CancellationToken cancellationToken)
        {
            var response = mapper.Map<ServerCustomStatisticsResponse>(command.Statistics);
            var serializedData = JsonSerializer.Serialize(response);
            await hubStatistics.Clients.Group(command.Key).ReceiveStatistics(command.Key, serializedData);

            return Unit.Value;
        }
    }
}
