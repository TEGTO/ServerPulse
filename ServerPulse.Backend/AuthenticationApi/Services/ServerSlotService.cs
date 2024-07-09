using AuthenticationApi.Data;
using AuthenticationApi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Services;

namespace AuthenticationApi.Services
{
    public class ServerSlotService : ServiceDbBase<AuthIdentityDbContext>, IServerSlotService
    {
        private readonly UserManager<User> userManager;

        public ServerSlotService(IDbContextFactory<AuthIdentityDbContext> contextFactory, UserManager<User> userManager) : base(contextFactory)
        {
            this.userManager = userManager;
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
        public async Task<ServerSlot?> GetServerSlotAsync(string email, string password, string serverSlotId, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null || !await userManager.CheckPasswordAsync(user, password))
            {
                throw new UnauthorizedAccessException("Invalid Authentication");
            }
            using (var dbContext = await CreateDbContextAsync(cancellationToken))
            {
                var result = await dbContext.ServerSlots.FirstOrDefaultAsync(x => x.Id == serverSlotId, cancellationToken);
                return result;
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
    }
}