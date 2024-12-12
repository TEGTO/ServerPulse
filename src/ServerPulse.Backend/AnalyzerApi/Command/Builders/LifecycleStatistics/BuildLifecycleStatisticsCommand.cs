using AnalyzerApi.Infrastructure.Models.Statistics;

namespace AnalyzerApi.Command.Builders.LifecycleStatistics
{
    public record BuildLifecycleStatisticsCommand(string Key) : BuildStatisticsCommand<ServerLifecycleStatistics>(Key);
}
