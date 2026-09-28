using Microsoft.EntityFrameworkCore;
using apitienda.Models;

namespace apitienda.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        public DbSet<Auditoria> Auditorias { get; set; }
    }
}
