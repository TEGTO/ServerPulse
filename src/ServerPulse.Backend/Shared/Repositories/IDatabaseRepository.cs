using Microsoft.EntityFrameworkCore;

namespace Shared.Repositories
{
    public interface IDatabaseRepository<TContext> where TContext : DbContext
    {
        public Task MigrateDatabaseAsync(CancellationToken cancelentionToken);
        public Task<TContext> CreateDbContextAsync(CancellationToken cancelentionToken);
    }
}
