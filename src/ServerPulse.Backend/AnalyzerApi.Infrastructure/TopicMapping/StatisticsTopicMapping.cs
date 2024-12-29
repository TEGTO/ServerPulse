using AnalyzerApi.Infrastructure.Models.Statistics;

namespace AnalyzerApi.Infrastructure.TopicMapping
{
    public record class StatisticsTopicMapping<TStatistics>(string TopicOriginName) where TStatistics : BaseStatistics;
}
