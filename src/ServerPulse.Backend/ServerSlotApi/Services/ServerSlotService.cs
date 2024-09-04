using Microsoft.EntityFrameworkCore;
using ServerSlotApi.Data;
using ServerSlotApi.Domain.Entities;
using Shared.Repositories;

namespace ServerSlotApi.Services
{
    public class ServerSlotService : IServerSlotService
    {
        private readonly IDatabaseRepository<ServerDataDbContext> repository;
        private readonly int slotsPerUser;

        public ServerSlotService(IDatabaseRepository<ServerDataDbContext> repository, IConfiguration configuration)
        {
            this.repository = repository;
            slotsPerUser = int.Parse(configuration[Configuration.SERVER_SLOTS_PER_USER]!);
        }

        #region IServerSlotService Members

        public async Task<ServerSlot?> GetSlotByIdAsync(SlotParams param, CancellationToken cancellationToken)
        {
            var slotQueryable = (await repository.GetQueryableAsync<ServerSlot>(cancellationToken)).AsNoTracking();
            var slot = await slotQueryable.FirstOrDefaultAsync(x =>
            x.UserEmail == param.UserEmail &&
            x.Id == param.SlotId,
            cancellationToken);
            return slot;
        }
        public async Task<IEnumerable<ServerSlot>> GetSlotsByEmailAsync(string email, CancellationToken cancellationToken)
        {
            var slotQueryable = (await repository.GetQueryableAsync<ServerSlot>(cancellationToken)).AsNoTracking();
            var slots = await slotQueryable.Where(x => x.UserEmail == email).OrderByDescending(x => x.CreationDate).ToListAsync();
            return slots;
        }
        public async Task<IEnumerable<ServerSlot>> GerSlotsContainingStringAsync(string email, string str, CancellationToken cancellationToken)
        {
            var slotQueryable = (await repository.GetQueryableAsync<ServerSlot>(cancellationToken)).AsNoTracking();
            var slots = await slotQueryable.Where(x =>
            x.UserEmail == email &&
            x.Name.Contains(str))
            .OrderByDescending(x => x.CreationDate).ToListAsync();
            return slots;
        }
        public async Task<bool> CheckIfKeyValidAsync(string key, CancellationToken cancellationToken)
        {
            var slotQueryable = (await repository.GetQueryableAsync<ServerSlot>(cancellationToken)).AsNoTracking();
            return await slotQueryable.AnyAsync(x => x.SlotKey == key);
        }
        public async Task<ServerSlot> CreateSlotAsync(ServerSlot serverSlot, CancellationToken cancellationToken)
        {
            return await repository.AddAsync(serverSlot, cancellationToken);
        }
        public async Task UpdateSlotAsync(SlotParams param, ServerSlot serverSlot, CancellationToken cancellationToken)
        {
            var slotQueryable = (await repository.GetQueryableAsync<ServerSlot>(cancellationToken)).AsNoTracking();
            var serverSlotInDb = await slotQueryable.FirstAsync(x =>
            x.Id == param.SlotId &&
            x.UserEmail == param.UserEmail,
            cancellationToken);
            serverSlotInDb.Copy(serverSlot);
            await repository.UpdateAsync(serverSlotInDb, cancellationToken);
        }
        public async Task DeleteSlotByIdAsync(SlotParams param, CancellationToken cancellationToken)
        {
            var slotQueryable = (await repository.GetQueryableAsync<ServerSlot>(cancellationToken)).AsNoTracking();
            var serverSlotInDb = await slotQueryable.FirstAsync(x =>
            x.Id == param.SlotId &&
            x.UserEmail == param.UserEmail,
            cancellationToken);
            await repository.DeleteAsync(serverSlotInDb, cancellationToken);
        }

        #endregion
    }
}