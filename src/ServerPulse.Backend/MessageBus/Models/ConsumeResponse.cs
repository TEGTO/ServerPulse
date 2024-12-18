#pragma warning disable CS8424 
namespace MessageBus.Models
{
    public record ConsumeResponse(string Message, DateTime CreationTimeUTC);
}