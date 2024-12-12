using AnalyzerApi.Infrastructure.Models.Statistics;

namespace AnalyzerApi.Command.Senders.LoadStatistics
{
    public record SendLoadStatisticsCommand(string Key, ServerLoadStatistics Statistics) : SendStatisticsCommand<ServerLoadStatistics>(Key, Statistics);
}
