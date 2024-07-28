﻿using Microsoft.EntityFrameworkCore;
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

        public async Task<ServerSlot?> GetSlotIdAsync(string id, CancellationToken cancellationToken)
        {
            ServerSlot? serverSlot = null;
            using (var dbContext = await CreateDbContextAsync(cancellationToken))
            {
                serverSlot = await dbContext.ServerSlots.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            }
            return serverSlot;
        }
        public async Task<IEnumerable<ServerSlot>> GetSlotsByEmailAsync(string email, CancellationToken cancellationToken)
        {
            List<ServerSlot> serverSlots = new List<ServerSlot>();
            using (var dbContext = await CreateDbContextAsync(cancellationToken))
            {
                var slots = dbContext.ServerSlots.Where(x => x.UserEmail == email)
                    .OrderByDescending(x => x.CreationDate).AsNoTracking();
                serverSlots.AddRange(slots);
            }
            return serverSlots;
        }
        public async Task<IEnumerable<ServerSlot>> GerSlotsContainingStringAsync(string email, string str, CancellationToken cancellationToken)
        {
            List<ServerSlot> serverSlots = new List<ServerSlot>();
            using (var dbContext = await CreateDbContextAsync(cancellationToken))
            {
                var slots = dbContext.ServerSlots.Where(x => x.UserEmail == email && x.Name.Contains(str))
                    .OrderByDescending(x => x.CreationDate).AsNoTracking();
                serverSlots.AddRange(slots);
            }
            return serverSlots;
        }
        public async Task<bool> CheckIfKeyValidAsync(string key, CancellationToken cancellationToken)
        {
            using (var dbContext = await CreateDbContextAsync(cancellationToken))
            {
                return await dbContext.ServerSlots.AnyAsync(x => x.SlotKey == key, cancellationToken);
            }
        }
        public async Task<ServerSlot> CreateSlotAsync(ServerSlot serverSlot, CancellationToken cancellationToken)
        {
            using (var dbContext = await CreateDbContextAsync(cancellationToken))
            {
                if (await dbContext.ServerSlots.CountAsync(x => x.UserEmail == serverSlot.UserEmail) > slotsPerUser)
                {
                    throw new InvalidOperationException("Too many slots per user!");
                }

                await dbContext.ServerSlots.AddAsync(serverSlot, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            return serverSlot;
        }
        public async Task UpdateSlotAsync(ServerSlot serverSlot, CancellationToken cancellationToken)
        {
            using (var dbContext = await CreateDbContextAsync(cancellationToken))
            {
                var serverSlotInDb = await dbContext.ServerSlots.FirstAsync(x => x.Id == serverSlot.Id, cancellationToken);
                serverSlotInDb.Copy(serverSlot);
                dbContext.ServerSlots.Update(serverSlotInDb);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
        public async Task DeleteSlotByIdAsync(string email, string id, CancellationToken cancellationToken)
        {
            using (var dbContext = await CreateDbContextAsync(cancellationToken))
            {
                var serverSlot = await dbContext.ServerSlots.FirstOrDefaultAsync(x => x.Id == id && x.UserEmail == email, cancellationToken);
                if (serverSlot != null)
                {
                    dbContext.ServerSlots.Remove(serverSlot);
                }
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
        private async Task<ServerDataDbContext> CreateDbContextAsync(CancellationToken cancellationToken)
        {
            return await repository.CreateDbContextAsync(cancellationToken);
        }
    }
}