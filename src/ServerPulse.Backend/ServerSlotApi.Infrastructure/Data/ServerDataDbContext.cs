using Microsoft.EntityFrameworkCore;
using ServerSlotApi.Infrastructure.Entities;

namespace ServerSlotApi.Infrastructure.Data
{
    public class ServerDataDbContext : DbContext
    {
        public virtual DbSet<ServerSlot> ServerSlots { get; set; }

        public ServerDataDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}
