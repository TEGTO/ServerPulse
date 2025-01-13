using ServerSlotApi.Core.Entities;
using ServerSlotApi.Core.Models;

namespace ServerSlotApi.Infrastructure.Repositories
{
    public interface IServerSlotRepository
    {
        public Task<ServerSlot> CreateSlotAsync(ServerSlot serverSlot, CancellationToken cancellationToken);
        public Task DeleteSlotAsync(ServerSlot serverSlot, CancellationToken cancellationToken);
        public Task<ServerSlot?> GetSlotAsync(GetSlot model, CancellationToken cancellationToken);
        public Task<ServerSlot?> GetSlotByKeyAsync(string key, CancellationToken cancellationToken);
        public Task<IEnumerable<ServerSlot>> GetSlotsByUserEmailAsync(string email, string str = "", CancellationToken cancellationToken = default);
        public Task UpdateSlotAsync(ServerSlot serverSlot, CancellationToken cancellationToken);
    }
}