using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using MediatR;

namespace AnalyzerApi.Command.Controllers.GetDailyLoadStatistics
{
    public record GetDailyLoadStatisticsQuery(string Key) : IRequest<IEnumerable<LoadAmountStatisticsResponse>>;
}
