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

        #region IDatabaseRepository Members

        public async Task MigrateDatabaseAsync(CancellationToken cancellationToken)
        {
            var dbContext = await CreateDbContextAsync(cancellationToken);
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        public async Task<T> AddAsync<T>(T obj, CancellationToken cancellationToken) where T : class
        {
            var dbContext = await CreateDbContextAsync(cancellationToken);
            await dbContext.AddAsync(obj, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return obj;
        }
        public async Task<IQueryable<T>> GetQueryableAsync<T>(CancellationToken cancellationToken) where T : class
        {
            var dbContext = await CreateDbContextAsync(cancellationToken);
            return dbContext.Set<T>().AsQueryable();
        }
        public async Task UpdateAsync<T>(T obj, CancellationToken cancellationToken) where T : class
        {
            var dbContext = await CreateDbContextAsync(cancellationToken);
            dbContext.Update(obj);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        public async Task DeleteAsync<T>(T obj, CancellationToken cancellationToken) where T : class
        {
            var dbContext = await CreateDbContextAsync(cancellationToken);
            dbContext.Remove(obj);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region Private Helpers

        private async Task<TContext> CreateDbContextAsync(CancellationToken cancelentionToken)
        {
            return await dbContextFactory.CreateDbContextAsync(cancelentionToken);
        }

        #endregion
    }
}
