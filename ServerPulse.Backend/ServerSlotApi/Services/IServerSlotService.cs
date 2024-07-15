using ServerSlotApi.Domain.Entities;

namespace ServerSlotApi.Services
{
    public interface IServerSlotService
    {
        public Task<IEnumerable<ServerSlot>> GetServerSlotsByEmailAsync(string email, CancellationToken cancellationToken);
        public Task<IEnumerable<ServerSlot>> GerServerSlotsContainingStringAsync(string email, string str, CancellationToken cancellationToken);
        public Task<ServerSlot> CreateServerSlotAsync(ServerSlot serverSlot, CancellationToken cancellationToken);
        public Task UpdateServerSlotAsync(ServerSlot serverSlot, CancellationToken cancellationToken);
        public Task DeleteServerSlotByIdAsync(string id, CancellationToken cancellationToken);
    }
}
