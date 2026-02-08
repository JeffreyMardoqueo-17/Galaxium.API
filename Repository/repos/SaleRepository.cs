using Microsoft.EntityFrameworkCore;
using Galaxium.API.Entities;
using Galaxium.Api.Repository.Interfaces;
using Galaxium.API.Data;

namespace Galaxium.API.Repositories.Implementations
{
    public class SaleRepository : ISaleRepository
    {
        private readonly GalaxiumDbContext _context;

        public SaleRepository(GalaxiumDbContext context)
        {
            _context = context;
        }

        // ============================================
        // Crear venta completa (cabecera + detalles + stock)
        // ============================================
        public async Task<Sale> CreateSaleWithDetailsAsync(Sale sale, IEnumerable<SaleDetail> saleDetails)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Guardar cabecera
                await _context.Sale.AddAsync(sale);
                await _context.SaveChangesAsync();

                // 2. Guardar detalles y actualizar stock
                foreach (var detail in saleDetails)
                {
                    detail.SaleId = sale.Id;
                    
                    // Agregar detalle (Product ya es null desde el Service)
                    await _context.SaleDetail.AddAsync(detail);

                    // 3. Cargar y actualizar el producto
                    var product = await _context.Product.FindAsync(detail.ProductId);
                    if (product != null)
                    {
                        // Descontar stock
                        product.Stock -= detail.Quantity;

                        // Inactivar producto si stock = 0
                        if (product.Stock <= 0)
                            product.IsActive = false;

                        // EF Core ya rastrea este producto desde FindAsync
                    }
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                
                // Recargar la venta con todos los detalles y valores calculados
                var savedSale = await _context.Sale
                    .Include(s => s.Customer)
                    .Include(s => s.User)
                    .Include(s => s.PaymentMethod)
                    .Include(s => s.Details)
                        .ThenInclude(d => d.Product)
                    .FirstOrDefaultAsync(s => s.Id == sale.Id);
                
                return savedSale ?? sale;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ============================================
        // Obtener venta por Id
        // ============================================
        public async Task<Sale?> GetByIdAsync(int saleId)
        {
            return await _context.Sale
                .Include(s => s.Customer)
                .Include(s => s.User)
                .Include(s => s.PaymentMethod)
                .Include(s => s.Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(s => s.Id == saleId);
        }

        // ============================================
        // Listar todas las ventas
        // ============================================
        public async Task<IEnumerable<Sale>> GetAllAsync()
        {
            return await _context.Sale
                .Include(s => s.Customer)
                .Include(s => s.User)
                .Include(s => s.PaymentMethod)
                .Include(s => s.Details)
                    .ThenInclude(d => d.Product)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        // ============================================
        // Ventas por rango de fechas
        // ============================================
        public async Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _context.Sale
                .Where(s => s.SaleDate >= start && s.SaleDate <= end)
                .Include(s => s.Customer)
                .Include(s => s.User)
                .Include(s => s.PaymentMethod)
                .Include(s => s.Details)
                    .ThenInclude(d => d.Product)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        // ============================================
        // Ventas por cliente
        // ============================================
        public async Task<IEnumerable<Sale>> GetByCustomerAsync(int customerId)
        {
            return await _context.Sale
                .Where(s => s.CustomerId == customerId)
                .Include(s => s.User)
                .Include(s => s.PaymentMethod)
                .Include(s => s.Details)
                    .ThenInclude(d => d.Product)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        // ============================================
        // Guardar cambios sueltos (opcional)
        // ============================================
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
