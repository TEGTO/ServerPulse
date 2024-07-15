﻿using Microsoft.EntityFrameworkCore;
using ServerSlotApi.Data;
using ServerSlotApi.Domain.Entities;
using Shared.Services;

namespace ServerSlotApi.Services
{
    public class ServerSlotService : ServiceDbBase<ServerDataDbContext>, IServerSlotService
    {
        private readonly IConfiguration configuration;

        public ServerSlotService(IDbContextFactory<ServerDataDbContext> contextFactory, IConfiguration configuration) : base(contextFactory)
        {
            this.configuration = configuration;
        }

        public async Task<IEnumerable<ServerSlot>> GetServerSlotsByEmailAsync(string email, CancellationToken cancellationToken)
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
        public async Task<IEnumerable<ServerSlot>> GerServerSlotsContainingStringAsync(string email, string str, CancellationToken cancellationToken)
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
        public async Task<ServerSlot> CreateServerSlotAsync(ServerSlot serverSlot, CancellationToken cancellationToken)
        {
            using (var dbContext = await CreateDbContextAsync(cancellationToken))
            {
                var slotsPerUser = int.Parse(configuration["SlotsPerUser"]);
                if (await dbContext.ServerSlots.CountAsync(x => x.UserEmail == serverSlot.UserEmail) > slotsPerUser)
                {
                    throw new InvalidOperationException("Too many slots per user!");
                }

                await dbContext.ServerSlots.AddAsync(serverSlot, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            return serverSlot;
        }
        public async Task UpdateServerSlotAsync(ServerSlot serverSlot, CancellationToken cancellationToken)
        {
            using (var dbContext = await CreateDbContextAsync(cancellationToken))
            {
                var serverSlotInDb = await dbContext.ServerSlots.FirstAsync(x => x.Id == serverSlot.Id, cancellationToken);
                serverSlotInDb.Copy(serverSlot);
                dbContext.ServerSlots.Update(serverSlotInDb);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
        public async Task DeleteServerSlotByIdAsync(string id, CancellationToken cancellationToken)
        {
            using (var dbContext = await CreateDbContextAsync(cancellationToken))
            {
                var serverSlot = await dbContext.ServerSlots.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                dbContext.ServerSlots.Remove(serverSlot);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}