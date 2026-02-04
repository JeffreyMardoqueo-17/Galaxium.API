using Microsoft.EntityFrameworkCore;
using Galaxium.API.Entities;
using Galaxium.Api.Entities;

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
        public DbSet<ProductPhoto> ProductPhoto => Set<ProductPhoto>();
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
        public DbSet<StockMovement> StockMovement => Set<StockMovement>();
        public DbSet<RefreshToken> RefreshToken => Set<RefreshToken>();
        public DbSet<StockEntry> StockEntry => Set<StockEntry>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.CostPrice).HasColumnType("decimal(18,2)");
                entity.Property(p => p.SalePrice).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Sale>(entity =>
            {
                entity.Property(s => s.Total).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<SaleDetail>(entity =>
            {
                entity.Property(sd => sd.SubTotal).HasColumnType("decimal(18,2)");
                entity.Property(sd => sd.UnitPrice).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<StockEntry>()
    .Property(x => x.TotalCost)
    .HasComputedColumnSql("[Quantity] * [UnitCost]", stored: true);

        }

    }
}
