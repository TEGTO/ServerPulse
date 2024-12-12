using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Requests;
using MediatR;

namespace AnalyzerApi.Command.Controllers.GetAmountStatisticsInRange
{
    public record GetAmountStatisticsInRangeQuery(MessageAmountInRangeRequest Request) : IRequest<IEnumerable<LoadAmountStatisticsResponse>>;
}
