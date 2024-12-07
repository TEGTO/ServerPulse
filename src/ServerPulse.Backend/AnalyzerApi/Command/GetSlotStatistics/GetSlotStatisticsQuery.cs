using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using MediatR;

namespace AnalyzerApi.Command.GetSlotStatistics
{
    public record GetSlotStatisticsQuery(string Key) : IRequest<SlotStatisticsResponse>;
}
