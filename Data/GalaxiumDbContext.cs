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
        public DbSet<PasswordResetCode> PasswordResetCode => Set<PasswordResetCode>();
        public DbSet<StockAlert> StockAlert => Set<StockAlert>();
        public DbSet<Supplier> Supplier => Set<Supplier>();
        public DbSet<Purchase> Purchase => Set<Purchase>();
        public DbSet<PurchaseDetail> PurchaseDetail => Set<PurchaseDetail>();

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
                entity.Property(p => p.UnitOfMeasure).HasConversion<string>().HasMaxLength(30);
            });

            // Venta
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.Property(s => s.SubTotal).HasColumnType("decimal(18,2)");
                entity.Property(s => s.Discount).HasColumnType("decimal(18,2)");
                entity.Property(s => s.AmountPaid).HasColumnType("decimal(18,2)");
                entity.Property(s => s.ChangeAmount).HasColumnType("decimal(18,2)");
                entity.Property(s => s.Total).HasColumnType("decimal(18,2)");
            });

            // Detalles de venta
            modelBuilder.Entity<SaleDetail>(entity =>
            {
                // SubTotal es columna calculada en PostgreSQL
                entity.Property(sd => sd.SubTotal)
                    .HasColumnType("decimal(18,2)")
                    .HasComputedColumnSql("\"Quantity\" * \"UnitPrice\"", stored: true);

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
            "\"Quantity\" * \"UnitCost\"",
            stored: true
        );

    entity.Property(e => e.IsActive)
        .HasDefaultValue(true);

    entity.Property(e => e.CreatedAt)
        .HasDefaultValueSql("NOW()");

    // Relaciones

    entity.HasOne(e => e.Product)
        .WithMany(p => p.StockEntries)
        .HasForeignKey(e => e.ProductId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(e => e.User)
        .WithMany(u => u.StockEntries)
        .HasForeignKey(e => e.UserId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(e => e.Supplier)
        .WithMany(s => s.StockEntries)
        .HasForeignKey(e => e.SupplierId)
        .OnDelete(DeleteBehavior.SetNull);
});

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.Property(s => s.Name).HasMaxLength(150);
                entity.Property(s => s.Phone).HasMaxLength(30);
                entity.Property(s => s.Email).HasMaxLength(150);
                entity.Property(s => s.Address).HasMaxLength(300);
            });

            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.Property(p => p.Total).HasColumnType("decimal(18,2)");
                entity.Property(p => p.Status).HasMaxLength(30);

                entity.HasOne(p => p.Supplier)
                    .WithMany(s => s.Purchases)
                    .HasForeignKey(p => p.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.User)
                    .WithMany()
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PurchaseDetail>(entity =>
            {
                entity.Property(p => p.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(p => p.Total).HasColumnType("decimal(18,2)");

                entity.HasOne(p => p.Purchase)
                    .WithMany(s => s.Details)
                    .HasForeignKey(p => p.PurchaseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Product)
                    .WithMany()
                    .HasForeignKey(p => p.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StockAlert>(entity =>
            {
                entity.Property(a => a.AlertType).HasConversion<string>().HasMaxLength(30);
                entity.Property(a => a.Message).HasMaxLength(300);
                entity.HasOne(a => a.Product)
                    .WithMany()
                    .HasForeignKey(a => a.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
});

            // Seed de roles base con IDs fijos
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Administrator" },
                new Role { Id = 2, Name = "Cashier" },
                new Role { Id = 3, Name = "Supervisor" }
            );
        }


    }
}
