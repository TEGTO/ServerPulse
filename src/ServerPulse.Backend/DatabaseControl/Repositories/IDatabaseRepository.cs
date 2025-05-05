using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace DatabaseControl.Repositories
{
    public interface IDatabaseRepository<TContext> where TContext : DbContext
    {
        public Task MigrateDatabaseAsync(CancellationToken cancellationToken);
        public Task<TContext> GetDbContextAsync(CancellationToken cancellationToken);
        public IQueryable<T> Query<T>(TContext dbContext) where T : class;
        public IQueryable<T> Query<T>(TContext dbContext, string setName) where T : class;
        public Task<T> AddAsync<T>(TContext dbContext, T obj, CancellationToken cancellationToken) where T : class;
        public T Update<T>(TContext dbContext, T obj) where T : class;
        public void Remove<T>(TContext dbContext, T obj) where T : class;
        public Task SaveChangesAsync(TContext dbContext, CancellationToken cancellationToken);
        public Task<IDbContextTransaction> BeginTransactionAsync(TContext dbContext, IsolationLevel isolationLevel, CancellationToken cancellationToken);
    }
}