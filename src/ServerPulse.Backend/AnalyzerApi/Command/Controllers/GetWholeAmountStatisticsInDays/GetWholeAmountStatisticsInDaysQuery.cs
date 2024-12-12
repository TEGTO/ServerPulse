using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using MediatR;

namespace AnalyzerApi.Command.Controllers.GetWholeAmountStatisticsInDays
{
    public record GetWholeAmountStatisticsInDaysQuery(string Key) : IRequest<IEnumerable<LoadAmountStatisticsResponse>>;
}
