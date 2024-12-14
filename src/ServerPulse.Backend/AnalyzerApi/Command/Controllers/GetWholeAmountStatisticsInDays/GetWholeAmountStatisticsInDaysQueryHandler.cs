using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Models.Statistics;
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

            var statisticsInRangeCollection = await receiver.GetWholeStatisticsInTimeSpanAsync(key, timeSpan, cancellationToken);

            var options = new GetInRangeOptions(key, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
            var todayStatisticsCollection = await receiver.GetStatisticsInRangeAsync(options, timeSpan, cancellationToken);

            var mergedStatisticsCollection = MergeStatisticsCollections(statisticsInRangeCollection, todayStatisticsCollection);

            return mergedStatisticsCollection.Select(mapper.Map<LoadAmountStatisticsResponse>);
        }

        private static IEnumerable<LoadAmountStatistics> MergeStatisticsCollections(
            IEnumerable<LoadAmountStatistics> statisticsInRangeCollection, IEnumerable<LoadAmountStatistics> todayStatisticsCollection)
        {
            var mergedStatisticsCollection = statisticsInRangeCollection
                .Where(x => !todayStatisticsCollection.Any(y => x.DateFrom > y.DateFrom || x.DateTo > y.DateFrom)).ToList();
            mergedStatisticsCollection.AddRange(todayStatisticsCollection);

            return mergedStatisticsCollection;
        }
    }
}
