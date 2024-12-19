using AnalyzerApi.Infrastructure.Models.Wrappers;

namespace AnalyzerApi.Infrastructure.Configurations
{
    public record class EventReceiverTopicConfiguration<TWrapper>(string TopicOriginName) where TWrapper : BaseEventWrapper;
}