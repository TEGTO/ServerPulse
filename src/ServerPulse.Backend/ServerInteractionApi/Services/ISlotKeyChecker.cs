
namespace ServerInteractionApi.Services
{
    public interface ISlotKeyChecker
    {
        public Task<bool> CheckSlotKeyAsync(string id, CancellationToken cancellationToken);
    }
}