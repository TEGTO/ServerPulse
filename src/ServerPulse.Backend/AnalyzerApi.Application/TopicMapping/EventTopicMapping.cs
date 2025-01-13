using AnalyzerApi.Core.Models.Wrappers;

namespace AnalyzerApi.Application.TopicMapping
{
    public record class EventTopicMapping<TWrapper>(string TopicOriginName) where TWrapper : BaseEventWrapper;
}