using AnalyzerApi.Infrastructure.Models.Statistics;

namespace AnalyzerApi.Infrastructure.Configurations
{
    public record class StatisticsReceiverTopicConfiguration<TStatistics>(string TopicOriginName) where TStatistics : BaseStatistics;
}
