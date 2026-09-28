using Microsoft.EntityFrameworkCore;
using apitienda.Models;

namespace apitienda.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options)
        {
        }

        public DbSet<Auditoria> Auditorias { get; set; }

        public DbSet<token_blacklist> token_blacklist { get; set; }
    }
}
