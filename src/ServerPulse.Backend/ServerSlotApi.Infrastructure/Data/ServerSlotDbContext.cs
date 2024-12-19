using Microsoft.EntityFrameworkCore;
using ServerSlotApi.Infrastructure.Entities;

namespace ServerSlotApi.Infrastructure.Data
{
    public class ServerSlotDbContext : DbContext
    {
        public virtual DbSet<ServerSlot> ServerSlots { get; set; }

        public ServerSlotDbContext(DbContextOptions options) : base(options)
        {
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<ServerSlot>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreationDate = DateTime.UtcNow;
                    entry.Entity.SlotKey = Guid.NewGuid().ToString();
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
