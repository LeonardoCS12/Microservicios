using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using productos.Models;
using Microsoft.EntityFrameworkCore.Design;


namespace productos.Data
{
    public class DataContextProduct : DbContext
    {
        public DataContextProduct(DbContextOptions<DataContextProduct>options):base(options){}
        public DbSet<Product> Products { get; set; }

    }

    public class DataContextProductFactory : IDesignTimeDbContextFactory<DataContextProduct>
    {
        public DataContextProduct CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DataContextProduct>();
            // Ponemos la cadena de conexión manual SOLO para la terminal (migraciones)
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=apitienda_dbdos;Username=postgres;Password=leo");
            return new DataContextProduct(optionsBuilder.Options);
        }
    }
}