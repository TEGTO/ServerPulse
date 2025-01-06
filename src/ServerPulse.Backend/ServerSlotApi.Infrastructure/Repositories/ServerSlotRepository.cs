using DatabaseControl.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ServerSlotApi.Infrastructure.Configuration;
using ServerSlotApi.Infrastructure.Data;
using ServerSlotApi.Infrastructure.Entities;
using ServerSlotApi.Infrastructure.Models;
using System.Data;

namespace ServerSlotApi.Infrastructure.Repositories
{
    public class ServerSlotRepository : IServerSlotRepository
    {
        private readonly IDatabaseRepository<ServerSlotDbContext> repository;
        private readonly int maxSlotsPerUser;

        public ServerSlotRepository(IDatabaseRepository<ServerSlotDbContext> repository, IConfiguration configuration)
        {
            this.repository = repository;
            maxSlotsPerUser = int.Parse(configuration[ConfigurationKeys.SERVER_SLOTS_PER_USER]!);
        }

        public async Task<ServerSlot?> GetSlotAsync(SlotModel model, CancellationToken cancellationToken)
        {
            using var dbContext = await repository.GetDbContextAsync(cancellationToken);
            var slotQueryable = repository.Query<ServerSlot>(dbContext);

            var slot = await slotQueryable
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserEmail == model.UserEmail && x.Id == model.SlotId, cancellationToken);

            return slot;
        }

        public async Task<ServerSlot?> GetSlotByKeyAsync(string key, CancellationToken cancellationToken)
        {
            using var dbContext = await repository.GetDbContextAsync(cancellationToken);
            var slotQueryable = repository.Query<ServerSlot>(dbContext);

            var slot = await slotQueryable
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SlotKey == key, cancellationToken);

            return slot;
        }

        public async Task<IEnumerable<ServerSlot>> GetSlotsByUserEmailAsync(string email, string str = "", CancellationToken cancellationToken = default)
        {
            using var dbContext = await repository.GetDbContextAsync(cancellationToken);
            var slotQueryable = repository.Query<ServerSlot>(dbContext);

            var slots = await slotQueryable
                .AsNoTracking()
                .Where(x => x.UserEmail == email && x.Name.ToLower().Contains(str.ToLower()))
                .OrderByDescending(x => x.CreationDate).ToListAsync(cancellationToken);

            return slots;
        }

        public async Task<ServerSlot> CreateSlotAsync(ServerSlot serverSlot, CancellationToken cancellationToken)
        {
            using var dbContext = await repository.GetDbContextAsync(cancellationToken);
            using var transaction = await repository.BeginTransactionAsync(dbContext, IsolationLevel.Serializable, cancellationToken);

            try
            {
                var userSlotAmount = await repository.Query<ServerSlot>(dbContext)
                    .Where(x => x.UserEmail == serverSlot.UserEmail)
                    .CountAsync(cancellationToken);

                if (userSlotAmount >= maxSlotsPerUser)
                {
                    throw new InvalidOperationException("Too many slots for a single user!");
                }

                var addedSlot = await repository.AddAsync(dbContext, serverSlot, cancellationToken);
                await repository.SaveChangesAsync(dbContext, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return addedSlot;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task UpdateSlotAsync(ServerSlot serverSlot, CancellationToken cancellationToken)
        {
            using var dbContext = await repository.GetDbContextAsync(cancellationToken);
            repository.Update(dbContext, serverSlot);
            await repository.SaveChangesAsync(dbContext, cancellationToken);
        }

        public async Task DeleteSlotAsync(ServerSlot serverSlot, CancellationToken cancellationToken)
        {
            using var dbContext = await repository.GetDbContextAsync(cancellationToken);
            repository.Remove(dbContext, serverSlot);
            await repository.SaveChangesAsync(dbContext, cancellationToken);
        }
    }
}
