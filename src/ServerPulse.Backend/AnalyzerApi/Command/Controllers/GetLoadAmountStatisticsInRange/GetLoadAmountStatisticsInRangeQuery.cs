using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Requests;
using MediatR;

namespace AnalyzerApi.Command.Controllers.GetLoadAmountStatisticsInRange
{
    public record GetLoadAmountStatisticsInRangeQuery(MessageAmountInRangeRequest Request) : IRequest<IEnumerable<LoadAmountStatisticsResponse>>;
}
