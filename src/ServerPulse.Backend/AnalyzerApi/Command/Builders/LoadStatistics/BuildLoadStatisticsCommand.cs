using AnalyzerApi.Infrastructure.Models.Statistics;

namespace AnalyzerApi.Command.Builders.LoadStatistics
{
    public record BuildLoadStatisticsCommand(string Key) : BuildStatisticsCommand<ServerLoadStatistics>(Key);
}
