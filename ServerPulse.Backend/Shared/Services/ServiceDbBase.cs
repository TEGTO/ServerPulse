using Microsoft.EntityFrameworkCore;

namespace Shared.Services
{
    public abstract class ServiceDbBase<T> where T : DbContext
    {
        private readonly IDbContextFactory<T> dbContextFactory;

        public ServiceDbBase(IDbContextFactory<T> contextFactory)
        {
            dbContextFactory = contextFactory;
        }
        protected async Task<T> CreateDbContextAsync(CancellationToken cancelentionToken)
        {
            return await dbContextFactory.CreateDbContextAsync(cancelentionToken);
        }
    }
}
