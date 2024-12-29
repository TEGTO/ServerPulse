using AnalyzerApi.Infrastructure.Models.Wrappers;

namespace AnalyzerApi.Infrastructure.TopicMapping
{
    public record class EventTopicMapping<TWrapper>(string TopicOriginName) where TWrapper : BaseEventWrapper;
}