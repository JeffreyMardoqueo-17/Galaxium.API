using Microsoft.EntityFrameworkCore;
using Galaxium.API.Entities;

namespace Galaxium.API.Data
{
    public class GalaxiumDbContext : DbContext
    {
        public GalaxiumDbContext(DbContextOptions<GalaxiumDbContext> options)
            : base(options)
        {
        }

        // ==========================
        // Seguridad
        // ==========================
        public DbSet<Role> Role => Set<Role>();
        public DbSet<User> User => Set<User>();

        // ==========================
        // Productos
        // ==========================
        public DbSet<ProductCategory> ProductCategory => Set<ProductCategory>();
        public DbSet<Product> Product => Set<Product>();

        // ==========================
        // Clientes
        // ==========================
        public DbSet<Customer> Customer => Set<Customer>();

        // ==========================
        // Ventas
        // ==========================
        public DbSet<Sale> Sale => Set<Sale>();
        public DbSet<SaleDetail> SaleDetails => Set<SaleDetail>();

        // ==========================
        // Inventario
        // ==========================
        public DbSet<StockMovement> StockMovements => Set<StockMovement>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones
        }
    }
}
