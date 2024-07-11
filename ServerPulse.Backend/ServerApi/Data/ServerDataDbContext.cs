using Microsoft.EntityFrameworkCore;
using ServerApi.Domain.Entities;

namespace ServerApi.Data
{
    public class ServerDataDbContext : DbContext
    {
        public DbSet<ServerSlot> ServerSlots { get; set; }

        public ServerDataDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}
