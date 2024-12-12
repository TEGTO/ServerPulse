using AnalyzerApi.Infrastructure.Models.Statistics;

namespace AnalyzerApi.Command.Senders.CustomStatistics
{
    public record SendCustomStatisticsCommand(string Key, ServerCustomStatistics Statistics) : SendStatisticsCommand<ServerCustomStatistics>(Key, Statistics);
}
