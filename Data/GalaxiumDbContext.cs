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
        public DbSet<SaleDetail> SaleDetail => Set<SaleDetail>();

        // ==========================
        // Inventario
        // ==========================
        public DbSet<RefreshToken> RefreshToken => Set<RefreshToken>();
        public DbSet<StockEntry> StockEntry => Set<StockEntry>();
        public DbSet<PaymentMethod> PaymentMethod => Set<PaymentMethod>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasMany(u => u.Sales)
                    .WithOne(s => s.User)
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.ProductsCreated)
                    .WithOne(p => p.CreatedByUser)
                    .HasForeignKey(p => p.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // SaleDetail does not have a UserId column in the schema.
                entity.Ignore(u => u.SaleDetails);
            });

            // Producto
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.CostPrice).HasColumnType("decimal(18,2)");
                entity.Property(p => p.SalePrice).HasColumnType("decimal(18,2)");
            });

            // Venta
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.Property(s => s.SubTotal).HasColumnType("decimal(18,2)");
                entity.Property(s => s.Discount).HasColumnType("decimal(18,2)");
                entity.Property(s => s.Total).HasColumnType("decimal(18,2)");
            });

            // Detalles de venta
            modelBuilder.Entity<SaleDetail>(entity =>
            {
                // SubTotal es columna calculada en SQL Server
                entity.Property(sd => sd.SubTotal)
                    .HasColumnType("decimal(18,2)")
                    .HasComputedColumnSql("([Quantity] * [UnitPrice])", stored: true);

                entity.Property(sd => sd.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(sd => sd.UnitCost).HasColumnType("decimal(18,2)");

                entity.HasOne(sd => sd.Sale)
                    .WithMany(s => s.Details)
                    .HasForeignKey(sd => sd.SaleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sd => sd.Product)
                    .WithMany(p => p.SaleDetails)
                    .HasForeignKey(sd => sd.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            // StockEntry
            modelBuilder.Entity<StockEntry>(entity =>
{
    entity.Property(e => e.UnitCost)
        .HasColumnType("decimal(18,2)");

    entity.Property(e => e.TotalCost)
        .HasColumnType("decimal(18,2)")
        .HasComputedColumnSql(
            "[Quantity] * [UnitCost]",
            stored: true
        );

    entity.Property(e => e.IsActive)
        .HasDefaultValue(true);

    entity.Property(e => e.CreatedAt)
        .HasDefaultValueSql("GETDATE()");

    // Relaciones

    entity.HasOne(e => e.Product)
        .WithMany(p => p.StockEntries)
        .HasForeignKey(e => e.ProductId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(e => e.User)
        .WithMany(u => u.StockEntries)
        .HasForeignKey(e => e.UserId)
        .OnDelete(DeleteBehavior.Restrict);
});

        }


    }
}
