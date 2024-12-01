using Microsoft.EntityFrameworkCore;

namespace DatabaseControl.Repositories
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2326:Unused type parameters should be removed", Justification = "<Pending>")]
    public interface IDatabaseRepository<TContext> where TContext : DbContext
    {
        public Task MigrateDatabaseAsync(CancellationToken cancellationToken);
        public Task<T> AddAsync<T>(T obj, CancellationToken cancellationToken) where T : class;
        public Task<IQueryable<T>> GetQueryableAsync<T>(CancellationToken cancellationToken) where T : class;
        public Task<T> UpdateAsync<T>(T obj, CancellationToken cancellationToken) where T : class;
        public Task UpdateRangeAsync<T>(IEnumerable<T> obj, CancellationToken cancellationToken) where T : class;
        public Task DeleteAsync<T>(T obj, CancellationToken cancellationToken) where T : class;
    }
}