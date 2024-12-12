using AnalyzerApi.Infrastructure.Models.Statistics;

namespace AnalyzerApi.Command.Builders.CustomStatistics
{
    public record BuildCustomStatisticsCommand(string Key) : BuildStatisticsCommand<ServerCustomStatistics>(Key);
}
