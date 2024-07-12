using Microsoft.EntityFrameworkCore;
using ServerSlotApi.Domain.Entities;

namespace ServerSlotApi.Data
{
    public class ServerDataDbContext : DbContext
    {
        public DbSet<ServerSlot> ServerSlots { get; set; }

        public ServerDataDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}
