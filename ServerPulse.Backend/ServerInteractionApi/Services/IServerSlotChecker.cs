
namespace ServerInteractionApi.Services
{
    public interface IServerSlotChecker
    {
        Task<bool> CheckServerSlotAsync(string id, CancellationToken cancellationToken);
    }
}