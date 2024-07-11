using Microsoft.EntityFrameworkCore;
using ServerApi.Data;
using ServerApi.Domain.Entities;
using Shared.Services;

namespace ServerApi.Services
{
    public class ServerSlotService : ServiceDbBase<ServerDataDbContext>, IServerSlotService
    {
        private readonly IAuthChecker authChecker;

        public ServerSlotService(IDbContextFactory<ServerDataDbContext> contextFactory, IAuthChecker authChecker) : base(contextFactory)
        {
            this.authChecker = authChecker;
        }

        public async Task<ServerSlot?> GetServerSlotAsync(string email, string password, string serverSlotId, CancellationToken cancellationToken)
        {
            var isCorrect = await authChecker.CheckAuthDataAsync(email, password, cancellationToken);
            if (isCorrect)
            {
                using (var dbContext = await CreateDbContextAsync(cancellationToken))
                {
                    var result = await dbContext.ServerSlots.FirstOrDefaultAsync(x => x.Id == serverSlotId, cancellationToken);
                    return result;
                }
            }
            else
            {
                throw new UnauthorizedAccessException("Invalid Authentication");
            }
        }
        public async Task<IEnumerable<ServerSlot>> GetServerSlotsByEmailAsync(string email, CancellationToken cancellationToken)
        {
            List<ServerSlot> serverSlots = new List<ServerSlot>();
            using (var dbContext = await CreateDbContextAsync(cancellationToken))
            {
                var slots = dbContext.ServerSlots.Where(x => x.UserEmail == email).AsNoTracking();
                serverSlots.AddRange(slots);
            }
            return serverSlots;
        }
        public async Task<ServerSlot> CreateServerSlotAsync(ServerSlot serverSlot, CancellationToken cancellationToken)
        {
            using (var dbContext = await CreateDbContextAsync(cancellationToken))
            {
                await dbContext.ServerSlots.AddAsync(serverSlot, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            return serverSlot;
        }
        public async Task<IEnumerable<ServerSlot>> GerServerSlotsContainingStringAsync(string email, string str, CancellationToken cancellationToken)
        {
            List<ServerSlot> serverSlots = new List<ServerSlot>();
            using (var dbContext = await CreateDbContextAsync(cancellationToken))
            {
                var slots = dbContext.ServerSlots.Where(x => x.UserEmail == email && x.Name.Contains(str)).AsNoTracking();
                serverSlots.AddRange(slots);
            }
            return serverSlots;
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