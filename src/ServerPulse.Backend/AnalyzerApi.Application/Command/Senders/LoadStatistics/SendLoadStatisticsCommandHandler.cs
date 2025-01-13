using AnalyzerApi.Application.Application.Services;
using AnalyzerApi.Core.Dtos.Responses.Statistics;
using AnalyzerApi.Core.Models.Statistics;
using AutoMapper;
using MediatR;

namespace AnalyzerApi.Application.Command.Senders.LoadStatistics
{
    internal class SendLoadStatisticsCommandHandler : IRequestHandler<SendStatisticsCommand<ServerLoadStatistics>, Unit>
    {
        private readonly IMapper mapper;
        private readonly IStatisticsNotifier<ServerLoadStatistics, ServerLoadStatisticsResponse> notifier;

        public SendLoadStatisticsCommandHandler(IStatisticsNotifier<ServerLoadStatistics, ServerLoadStatisticsResponse> notifier, IMapper mapper)
        {
            this.notifier = notifier;
            this.mapper = mapper;
        }

        public async Task<Unit> Handle(SendStatisticsCommand<ServerLoadStatistics> command, CancellationToken cancellationToken)
        {
            var response = mapper.Map<ServerLoadStatisticsResponse>(command.Statistics);
            await notifier.NotifyGroupAsync(command.Key, response);

            return Unit.Value;
        }
    }
}
