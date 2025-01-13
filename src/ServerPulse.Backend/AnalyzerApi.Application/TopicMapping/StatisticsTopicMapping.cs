using AnalyzerApi.Core.Models.Statistics;

namespace AnalyzerApi.Application.TopicMapping
{
    public record class StatisticsTopicMapping<TStatistics>(string TopicOriginName) where TStatistics : BaseStatistics;
}
