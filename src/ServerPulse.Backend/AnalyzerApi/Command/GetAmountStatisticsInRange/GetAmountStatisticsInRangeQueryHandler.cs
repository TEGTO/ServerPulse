using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Services.Interfaces;
using AutoMapper;
using MediatR;

namespace AnalyzerApi.Command.GetAmountStatisticsInRange
{
    public class GetAmountStatisticsInRangeQueryHandler : IRequestHandler<GetAmountStatisticsInRangeQuery, IEnumerable<LoadAmountStatisticsResponse>>
    {
        private readonly IStatisticsReceiver<LoadAmountStatistics> loadAmountStatisticsReceiver;
        private readonly IMapper mapper;

        public GetAmountStatisticsInRangeQueryHandler(IStatisticsReceiver<LoadAmountStatistics> loadAmountStatisticsReceiver, IMapper mapper)
        {
            this.loadAmountStatisticsReceiver = loadAmountStatisticsReceiver;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<LoadAmountStatisticsResponse>> Handle(GetAmountStatisticsInRangeQuery command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var options = new InRangeQueryOptions(request.Key, request.From.ToUniversalTime(), request.To.ToUniversalTime());
            var statistics = await loadAmountStatisticsReceiver.GetStatisticsInRangeAsync(options, request.TimeSpan, cancellationToken);

            return statistics.Select(mapper.Map<LoadAmountStatisticsResponse>);
        }
    }
}
