using AnalyzerApi.Application.Application.Services;
using AnalyzerApi.Core.Dtos.Responses.Statistics;
using AnalyzerApi.Core.Models.Statistics;
using AutoMapper;
using MediatR;

namespace AnalyzerApi.Application.Command.Senders.CustomStatistics
{
    internal class SendCustomStatisticsCommandHandler : IRequestHandler<SendStatisticsCommand<ServerCustomStatistics>, Unit>
    {
        private readonly IStatisticsNotifier<ServerCustomStatistics, ServerCustomStatisticsResponse> notifier;
        private readonly IMapper mapper;

        public SendCustomStatisticsCommandHandler(IStatisticsNotifier<ServerCustomStatistics, ServerCustomStatisticsResponse> notifier, IMapper mapper)
        {
            this.notifier = notifier;
            this.mapper = mapper;
        }

        public async Task<Unit> Handle(SendStatisticsCommand<ServerCustomStatistics> command, CancellationToken cancellationToken)
        {
            var response = mapper.Map<ServerCustomStatisticsResponse>(command.Statistics);
            await notifier.NotifyGroupAsync(command.Key, response);

            return Unit.Value;
        }
    }
}
