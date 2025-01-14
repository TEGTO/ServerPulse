using AnalyzerApi.Core.Models.Statistics;
using MediatR;

namespace AnalyzerApi.Application.Command.Builders
{
    public record BuildStatisticsCommand<TStatistics>(string Key) : IRequest<TStatistics> where TStatistics : BaseStatistics;
}
