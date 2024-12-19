using AnalyzerApi.Hubs;
using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace AnalyzerApi.Command.Senders.CustomStatistics
{
    public class SendCustomStatisticsCommandHandler : IRequestHandler<SendStatisticsCommand<ServerCustomStatistics>, Unit>
    {
        private readonly IHubContext<StatisticsHub<ServerCustomStatistics>, IStatisticsHubClient> hubStatistics;
        private readonly IMapper mapper;

        public SendCustomStatisticsCommandHandler(IHubContext<StatisticsHub<ServerCustomStatistics>, IStatisticsHubClient> hubStatistics, IMapper mapper)
        {
            this.hubStatistics = hubStatistics;
            this.mapper = mapper;
        }

        public async Task<Unit> Handle(SendStatisticsCommand<ServerCustomStatistics> command, CancellationToken cancellationToken)
        {
            var response = mapper.Map<ServerCustomStatisticsResponse>(command.Statistics);
            await hubStatistics.Clients.Group(command.Key).ReceiveStatistics(command.Key, response);

            return Unit.Value;
        }
    }
}
