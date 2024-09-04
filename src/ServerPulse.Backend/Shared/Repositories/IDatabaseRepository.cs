using Microsoft.EntityFrameworkCore;

namespace Shared.Repositories
{
    public interface IDatabaseRepository<TContext> where TContext : DbContext
    {
        public Task MigrateDatabaseAsync(CancellationToken cancelentionToken);
        public Task<T> AddAsync<T>(T obj, CancellationToken cancellationToken) where T : class;
        public Task<IQueryable<T>> GetQueryableAsync<T>(CancellationToken cancellationToken) where T : class;
        public Task UpdateAsync<T>(T obj, CancellationToken cancellationToken) where T : class;
        public Task DeleteAsync<T>(T obj, CancellationToken cancellationToken) where T : class;
    }
}