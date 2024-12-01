using ServerSlotApi.Infrastructure.Entities;

namespace ServerSlotApi.Services
{
    public record class SlotParams(string UserEmail, string SlotId);
    public interface IServerSlotService
    {
        public Task<ServerSlot?> GetSlotByIdAsync(SlotParams param, CancellationToken cancellationToken);
        public Task<IEnumerable<ServerSlot>> GetSlotsByEmailAsync(string email, CancellationToken cancellationToken);
        public Task<IEnumerable<ServerSlot>> GetSlotsContainingStringAsync(string email, string str, CancellationToken cancellationToken);
        public Task<bool> CheckIfKeyValidAsync(string key, CancellationToken cancellationToken);
        public Task<ServerSlot> CreateSlotAsync(ServerSlot serverSlot, CancellationToken cancellationToken);
        public Task UpdateSlotAsync(SlotParams param, ServerSlot serverSlot, CancellationToken cancellationToken);
        public Task DeleteSlotByIdAsync(SlotParams param, CancellationToken cancellationToken);
    }
}