#pragma warning disable CS8424 
namespace MessageBus.Models
{
    public record GetSomeMessagesFromDateOptions(string TopicName, int TimeoutInMilliseconds, int NumberOfMessages, DateTime StartDate, bool ReadNew = false);
}