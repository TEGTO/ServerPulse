using ServerSlotApi.Domain.Entities;

namespace ServerSlotApi.Services
{
    public interface IServerSlotService
    {
        public Task<ServerSlot?> GetSlotIdAsync(string id, CancellationToken cancellationToken);
        public Task<IEnumerable<ServerSlot>> GetSlotsByEmailAsync(string email, CancellationToken cancellationToken);
        public Task<IEnumerable<ServerSlot>> GerSlotsContainingStringAsync(string email, string str, CancellationToken cancellationToken);
        public Task<bool> CheckIfKeyValidAsync(string key, CancellationToken cancellationToken);
        public Task<ServerSlot> CreateSlotAsync(ServerSlot serverSlot, CancellationToken cancellationToken);
        public Task UpdateSlotAsync(ServerSlot serverSlot, CancellationToken cancellationToken);
        public Task DeleteSlotByIdAsync(string email, string id, CancellationToken cancellationToken);
    }
}