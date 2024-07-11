using AuthenticationApi.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationApi.Data
{
    public class AuthIdentityDbContext : IdentityDbContext<User>
    {
        public AuthIdentityDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<User>(u =>
            {
                u.HasIndex(u => u.Email).IsUnique();
            });
        }
    }
}
