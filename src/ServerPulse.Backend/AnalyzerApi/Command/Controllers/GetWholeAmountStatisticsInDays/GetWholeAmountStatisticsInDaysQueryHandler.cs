using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Services.Receivers.Statistics;
using AutoMapper;
using MediatR;

namespace AnalyzerApi.Command.Controllers.GetWholeAmountStatisticsInDays
{
    public class GetWholeAmountStatisticsInDaysQueryHandler : IRequestHandler<GetWholeAmountStatisticsInDaysQuery, IEnumerable<LoadAmountStatisticsResponse>>
    {
        private readonly ILoadAmountStatisticsReceiver receiver;
        private readonly IMapper mapper;

        public GetWholeAmountStatisticsInDaysQueryHandler(ILoadAmountStatisticsReceiver loadAmountStatisticsReceiver, IMapper mapper)
        {
            this.receiver = loadAmountStatisticsReceiver;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<LoadAmountStatisticsResponse>> Handle(GetWholeAmountStatisticsInDaysQuery command, CancellationToken cancellationToken)
        {
            var key = command.Key;

            var timeSpan = TimeSpan.FromDays(1);

            var statistics = await receiver.GetWholeStatisticsInTimeSpanAsync(key, timeSpan, cancellationToken);

            var options = new InRangeQuery(key, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
            var todayStatistics = await receiver.GetStatisticsInRangeAsync(options, timeSpan, cancellationToken);

            var response = statistics.Where(x => !todayStatistics.Any(y => x.DateFrom > y.DateFrom || x.DateTo > y.DateFrom)).ToList();
            response.AddRange(todayStatistics);

            return response.Select(mapper.Map<LoadAmountStatisticsResponse>);
        }
    }
}
