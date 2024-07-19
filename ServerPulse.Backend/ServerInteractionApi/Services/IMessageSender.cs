
namespace ServerInteractionApi.Services
{
    public interface IMessageSender
    {
        public Task SendAliveEventAsync(string slotId);
    }
}