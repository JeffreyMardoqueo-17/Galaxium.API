using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Galaxium.API.Entities;
using Galaxium.Api.Entities;
using Galaxium.Api.Shared.MultiTenant;

namespace Galaxium.API.Data
{
    public class GalaxiumDbContext : DbContext
    {
        public GalaxiumDbContext(DbContextOptions<GalaxiumDbContext> options)
            : base(options)
        {
        }

        // ==========================
        // MultiTenant
        // ==========================
        public int CurrentTenantId => TenantContext.TryGetTenantId() ?? 0;

        // ==========================
        // Seguridad
        // ==========================
        public DbSet<Tenant> Tenant => Set<Tenant>();
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
        // Proveedores
        // ==========================
        public DbSet<Supplier> Supplier => Set<Supplier>();

        // ==========================
        // Ventas
        // ==========================
        public DbSet<Sale> Sale => Set<Sale>();
        public DbSet<SaleDetail> SaleDetail => Set<SaleDetail>();

        // ==========================
        // Compras
        // ==========================
        public DbSet<Purchase> Purchase => Set<Purchase>();
        public DbSet<PurchaseDetail> PurchaseDetail => Set<PurchaseDetail>();

        // ==========================
        // Inventario
        // ==========================
        public DbSet<StockEntry> StockEntry => Set<StockEntry>();
        public DbSet<StockAlert> StockAlert => Set<StockAlert>();

        // ==========================
        // Métodos de pago
        // ==========================
        public DbSet<PaymentMethod> PaymentMethod => Set<PaymentMethod>();

        // ==========================
        // Autenticación
        // ==========================
        public DbSet<RefreshToken> RefreshToken => Set<RefreshToken>();
        public DbSet<PasswordResetCode> PasswordResetCode => Set<PasswordResetCode>();

        // ==========================
        // SaveChanges Override — Auto-set TenantId + Audit
        // ==========================
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyTenantId();
            ApplyAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            ApplyTenantId();
            ApplyAuditFields();
            return base.SaveChanges();
        }

        private void ApplyTenantId()
        {
            var tenantId = CurrentTenantId;
            if (tenantId == 0) return;

            foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
            {
                if (entry.State == EntityState.Added && entry.Entity.TenantId == 0)
                {
                    entry.Entity.TenantId = tenantId;
                }
            }
        }

        private void ApplyAuditFields()
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added)
                {
                    var createdAtProp = entry.Properties
                        .FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
                    if (createdAtProp != null && createdAtProp.CurrentValue == null)
                    {
                        createdAtProp.CurrentValue = now;
                    }
                }

                if (entry.State == EntityState.Modified)
                {
                    if (entry.Entity is Tenant tenant)
                    {
                        tenant.UpdatedAt = now;
                    }
                }
            }
        }

        // ==========================
        // Global Query Filters + Model Configuration
        // ==========================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply Global Query Filters for all ITenantEntity types (except Tenant itself)
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType)
                    && entityType.ClrType != typeof(Tenant))
                {
                    var method = typeof(GalaxiumDbContext)
                        .GetMethod(nameof(ApplyTenantQueryFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                        .MakeGenericMethod(entityType.ClrType);
                    method.Invoke(null, new object[] { modelBuilder });
                }
            }

            // ==========================
            // Tenant
            // ==========================
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Name).HasMaxLength(150);
                entity.HasIndex(t => t.Slug).IsUnique().HasFilter(null);
                entity.HasIndex(t => t.IsActive);
                entity.Ignore(t => t.TenantId);
            });

            // ==========================
            // Role (per-tenant)
            // ==========================
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(r => new { r.TenantId, r.Name }).IsUnique();

                entity.HasOne(r => r.Tenant)
                    .WithMany(t => t.Roles)
                    .HasForeignKey(r => r.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================
            // User
            // ==========================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => new { u.TenantId, u.Username }).IsUnique();
                entity.HasIndex(u => new { u.TenantId, u.Email }).IsUnique().HasFilter(null);

                entity.HasOne(u => u.Tenant)
                    .WithMany(t => t.Users)
                    .HasForeignKey(u => u.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(u => u.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.Sales)
                    .WithOne(s => s.User)
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.ProductsCreated)
                    .WithOne(p => p.CreatedByUser)
                    .HasForeignKey(p => p.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Ignore(u => u.SaleDetails);
            });

            // ==========================
            // ProductCategory (per-tenant)
            // ==========================
            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.HasIndex(c => new { c.TenantId, c.Name }).IsUnique();

                entity.HasOne(c => c.Tenant)
                    .WithMany(t => t.ProductCategories)
                    .HasForeignKey(c => c.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================
            // Product
            // ==========================
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(p => new { p.TenantId, p.SKU }).IsUnique();
                entity.HasIndex(p => new { p.TenantId, p.CategoryId });

                entity.Property(p => p.CostPrice).HasColumnType("decimal(18,2)");
                entity.Property(p => p.SalePrice).HasColumnType("decimal(18,2)");
                entity.Property(p => p.UnitOfMeasure).HasConversion<string>().HasMaxLength(30);

                entity.HasOne(p => p.Tenant)
                    .WithMany(t => t.Products)
                    .HasForeignKey(p => p.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.CreatedByUser)
                    .WithMany(u => u.ProductsCreated)
                    .HasForeignKey(p => p.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================
            // ProductPhoto
            // ==========================
            modelBuilder.Entity<ProductPhoto>(entity =>
            {
                entity.HasOne(pp => pp.Product)
                    .WithMany(p => p.Photos)
                    .HasForeignKey(pp => pp.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(typeof(Tenant))
                    .WithMany()
                    .HasForeignKey("TenantId")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================
            // Customer (per-tenant)
            // ==========================
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasIndex(c => new { c.TenantId, c.Email }).IsUnique().HasFilter(null);

                entity.HasOne(c => c.Tenant)
                    .WithMany(t => t.Customers)
                    .HasForeignKey(c => c.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================
            // Supplier (per-tenant)
            // ==========================
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasIndex(s => new { s.TenantId, s.Name }).IsUnique();

                entity.Property(s => s.Name).HasMaxLength(150);
                entity.Property(s => s.Phone).HasMaxLength(30);
                entity.Property(s => s.Email).HasMaxLength(150);
                entity.Property(s => s.Address).HasMaxLength(300);

                entity.HasOne(s => s.Tenant)
                    .WithMany(t => t.Suppliers)
                    .HasForeignKey(s => s.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================
            // PaymentMethod (per-tenant)
            // ==========================
            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.HasIndex(pm => new { pm.TenantId, pm.Name }).IsUnique();

                entity.HasOne(pm => pm.Tenant)
                    .WithMany(t => t.PaymentMethods)
                    .HasForeignKey(pm => pm.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================
            // Sale
            // ==========================
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.HasIndex(s => new { s.TenantId, s.SaleDate });
                entity.HasIndex(s => new { s.TenantId, s.Status });
                entity.HasIndex(s => s.UserId);

                entity.Property(s => s.SubTotal).HasColumnType("decimal(18,2)");
                entity.Property(s => s.Discount).HasColumnType("decimal(18,2)");
                entity.Property(s => s.AmountPaid).HasColumnType("decimal(18,2)");
                entity.Property(s => s.ChangeAmount).HasColumnType("decimal(18,2)");
                entity.Property(s => s.Total).HasColumnType("decimal(18,2)");

                entity.HasOne(s => s.Tenant)
                    .WithMany()
                    .HasForeignKey(s => s.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Customer)
                    .WithMany(c => c.Sales)
                    .HasForeignKey(s => s.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.User)
                    .WithMany(u => u.Sales)
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.PaymentMethod)
                    .WithMany()
                    .HasForeignKey(s => s.PaymentMethodId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================
            // SaleDetail
            // ==========================
            modelBuilder.Entity<SaleDetail>(entity =>
            {
                entity.HasIndex(sd => sd.SaleId);
                entity.HasIndex(sd => new { sd.TenantId, sd.ProductId });

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

                entity.HasOne(typeof(Tenant))
                    .WithMany()
                    .HasForeignKey("TenantId")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================
            // StockEntry
            // ==========================
            modelBuilder.Entity<StockEntry>(entity =>
            {
                entity.HasIndex(se => new { se.TenantId, se.ProductId });
                entity.HasIndex(se => new { se.ProductId, se.CreatedAt });

                entity.Property(e => e.UnitCost).HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalCost)
                    .HasColumnType("decimal(18,2)")
                    .HasComputedColumnSql("\"Quantity\" * \"UnitCost\"", stored: true);

                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);

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

            // ==========================
            // StockAlert
            // ==========================
            modelBuilder.Entity<StockAlert>(entity =>
            {
                entity.HasIndex(sa => new { sa.TenantId, sa.ProductId, sa.AlertType });
                entity.HasIndex(sa => new { sa.TenantId, sa.IsActive });

                entity.Property(a => a.AlertType).HasConversion<string>().HasMaxLength(30);
                entity.Property(a => a.Message).HasMaxLength(300);

                entity.HasOne(a => a.Product)
                    .WithMany(p => p.StockAlerts)
                    .HasForeignKey(a => a.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================
            // Purchase
            // ==========================
            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.HasIndex(p => new { p.TenantId, p.PurchaseDate });

                entity.Property(p => p.Total).HasColumnType("decimal(18,2)");
                entity.Property(p => p.Status).HasMaxLength(30);

                entity.HasOne(p => p.Tenant)
                    .WithMany()
                    .HasForeignKey(p => p.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Supplier)
                    .WithMany(s => s.Purchases)
                    .HasForeignKey(p => p.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.User)
                    .WithMany()
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================
            // PurchaseDetail
            // ==========================
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

                entity.HasOne(typeof(Tenant))
                    .WithMany()
                    .HasForeignKey("TenantId")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================
            // RefreshToken
            // ==========================
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(rt => rt.Token).IsUnique();
                entity.HasIndex(rt => new { rt.UserId, rt.IsRevoked });

                entity.HasOne(rt => rt.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(typeof(Tenant))
                    .WithMany()
                    .HasForeignKey("TenantId")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================
            // PasswordResetCode
            // ==========================
            modelBuilder.Entity<PasswordResetCode>(entity =>
            {
                entity.HasOne(prc => prc.User)
                    .WithMany()
                    .HasForeignKey(prc => prc.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(typeof(Tenant))
                    .WithMany()
                    .HasForeignKey("TenantId")
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        // ==========================
        // Dynamic Query Filter Builder
        // ==========================
        private static void ApplyTenantQueryFilter<T>(ModelBuilder modelBuilder) where T : class, ITenantEntity
        {
            var parameter = Expression.Parameter(typeof(T), "e");
            var tenantIdProperty = Expression.Property(parameter, nameof(ITenantEntity.TenantId));

            // Use TryGetTenantId() to avoid exceptions when TenantContext is not initialized.
            // Fallback to -1 (sentinel) so the filter returns no rows instead of throwing.
            var tryGetTenantIdMethod = typeof(TenantContext).GetMethod(nameof(TenantContext.TryGetTenantId))!;
            var callTryGet = Expression.Call(null, tryGetTenantIdMethod);
            var nullableTenantId = Expression.Convert(callTryGet, typeof(int?));
            var fallbackValue = Expression.Constant(-1, typeof(int));
            var currentTenantId = Expression.Coalesce(nullableTenantId, fallbackValue);

            var comparison = Expression.Equal(tenantIdProperty, currentTenantId);
            var lambda = Expression.Lambda(comparison, parameter);

            modelBuilder.Entity<T>().HasQueryFilter(lambda);
        }
    }
}
