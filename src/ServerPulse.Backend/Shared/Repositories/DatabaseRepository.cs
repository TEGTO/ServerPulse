using Microsoft.EntityFrameworkCore;

namespace Shared.Repositories
{
    public class DatabaseRepository<TContext> : IDatabaseRepository<TContext> where TContext : DbContext
    {
        private readonly IDbContextFactory<TContext> dbContextFactory;

        public DatabaseRepository(IDbContextFactory<TContext> contextFactory)
        {
            dbContextFactory = contextFactory;
        }

        public async Task MigrateDatabaseAsync(CancellationToken cancelentionToken)
        {
            using (var dbContext = await CreateDbContextAsync(cancelentionToken))
            {
                await dbContext.Database.MigrateAsync();
            }
        }
        public async Task<TContext> CreateDbContextAsync(CancellationToken cancelentionToken)
        {
            return await dbContextFactory.CreateDbContextAsync(cancelentionToken);
        }
    }
}
