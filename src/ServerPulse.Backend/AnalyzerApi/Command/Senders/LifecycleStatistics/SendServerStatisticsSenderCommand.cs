using AnalyzerApi.Infrastructure.Models.Statistics;

namespace AnalyzerApi.Command.Senders.LifecycleStatistics
{
    public record SendServerStatisticsSenderCommand(string Key, ServerLifecycleStatistics Statistics) : SendStatisticsCommand<ServerLifecycleStatistics>(Key, Statistics);
}
