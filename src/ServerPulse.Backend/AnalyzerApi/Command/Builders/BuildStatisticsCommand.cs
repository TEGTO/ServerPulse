using AnalyzerApi.Infrastructure.Models.Statistics;
using MediatR;

namespace AnalyzerApi.Command.Builders
{
    public record BuildStatisticsCommand<TStatistics>(string Key) : IRequest<TStatistics> where TStatistics : BaseStatistics;
}
