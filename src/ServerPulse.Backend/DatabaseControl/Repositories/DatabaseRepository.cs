using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Registry;

namespace DatabaseControl.Repositories
{
    public class DatabaseRepository<TContext> : IDatabaseRepository<TContext> where TContext : DbContext
    {
        private readonly IDbContextFactory<TContext> dbContextFactory;
        private readonly ResiliencePipeline resiliencePipeline;

        public DatabaseRepository(IDbContextFactory<TContext> dbContextFactory, ResiliencePipelineProvider<string> resiliencePipelineProvider)
        {
            this.dbContextFactory = dbContextFactory;
            resiliencePipeline = resiliencePipelineProvider.GetPipeline(DatabaseConfiguration.REPOSITORY_RESILIENCE_PIPELINE);
        }

        #region IDatabaseRepository Members

        public async Task MigrateDatabaseAsync(CancellationToken cancellationToken)
        {
            await resiliencePipeline.ExecuteAsync(async ct =>
            {
                var dbContext = await CreateDbContextAsync(ct).ConfigureAwait(false);
                await dbContext.Database.MigrateAsync(ct).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);
        }

        public async Task<T> AddAsync<T>(T obj, CancellationToken cancellationToken) where T : class
        {
            return await resiliencePipeline.ExecuteAsync(async ct =>
            {
                var dbContext = await CreateDbContextAsync(ct).ConfigureAwait(false);
                await dbContext.AddAsync(obj, ct).ConfigureAwait(false);
                await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
                return obj;
            }, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IQueryable<T>> GetQueryableAsync<T>(CancellationToken cancellationToken) where T : class
        {
            return await resiliencePipeline.ExecuteAsync(async ct =>
            {
                var dbContext = await CreateDbContextAsync(ct).ConfigureAwait(false);
                return dbContext.Set<T>().AsQueryable();
            }, cancellationToken).ConfigureAwait(false);
        }

        public async Task<T> UpdateAsync<T>(T obj, CancellationToken cancellationToken) where T : class
        {
            return await resiliencePipeline.ExecuteAsync(async ct =>
            {
                var dbContext = await CreateDbContextAsync(ct).ConfigureAwait(false);
                dbContext.Update(obj);
                await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
                return obj;
            }, cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateRangeAsync<T>(IEnumerable<T> obj, CancellationToken cancellationToken) where T : class
        {
            await resiliencePipeline.ExecuteAsync(async ct =>
            {
                var dbContext = await CreateDbContextAsync(ct).ConfigureAwait(false);
                dbContext.UpdateRange(obj);
                await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
                return obj;
            }, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync<T>(T obj, CancellationToken cancellationToken) where T : class
        {
            await resiliencePipeline.ExecuteAsync(async ct =>
            {
                var dbContext = await CreateDbContextAsync(ct).ConfigureAwait(false);
                dbContext.Remove(obj);
                await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);
        }

        #endregion

        #region Protected Helpers

        protected async Task<TContext> CreateDbContextAsync(CancellationToken cancellationToken)
        {
            return await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        }

        #endregion
    }
}
