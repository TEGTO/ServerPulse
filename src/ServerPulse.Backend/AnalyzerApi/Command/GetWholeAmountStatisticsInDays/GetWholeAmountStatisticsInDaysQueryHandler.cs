using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Services.Interfaces;
using AutoMapper;
using MediatR;

namespace AnalyzerApi.Command.GetWholeAmountStatisticsInDays
{
    public class GetWholeAmountStatisticsInDaysQueryHandler : IRequestHandler<GetWholeAmountStatisticsInDaysQuery, IEnumerable<LoadAmountStatisticsResponse>>
    {
        private readonly IStatisticsReceiver<LoadAmountStatistics> loadAmountStatisticsReceiver;
        private readonly IMapper mapper;

        public GetWholeAmountStatisticsInDaysQueryHandler(IStatisticsReceiver<LoadAmountStatistics> loadAmountStatisticsReceiver, IMapper mapper)
        {
            this.loadAmountStatisticsReceiver = loadAmountStatisticsReceiver;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<LoadAmountStatisticsResponse>> Handle(GetWholeAmountStatisticsInDaysQuery command, CancellationToken cancellationToken)
        {
            var key = command.Key;

            var timeSpan = TimeSpan.FromDays(1);

            var statistics = await loadAmountStatisticsReceiver.GetWholeStatisticsInTimeSpanAsync(key, timeSpan, cancellationToken);

            var options = new InRangeQueryOptions(key, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
            var todayStatistics = await loadAmountStatisticsReceiver.GetStatisticsInRangeAsync(options, timeSpan, cancellationToken);

            var response = statistics.Where(x => !todayStatistics.Any(y => x.DateFrom > y.DateFrom || x.DateTo > y.DateFrom)).ToList();
            response.AddRange(todayStatistics);

            return response.Select(mapper.Map<LoadAmountStatisticsResponse>);
        }
    }
}
