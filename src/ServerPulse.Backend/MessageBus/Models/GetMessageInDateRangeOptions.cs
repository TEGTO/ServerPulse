#pragma warning disable CS8424 
namespace MessageBus.Models
{
    public record GetMessageInDateRangeOptions(string TopicName, int TimeoutInMilliseconds, DateTime From, DateTime To);
}