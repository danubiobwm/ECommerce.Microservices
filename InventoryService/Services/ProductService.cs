using Microsoft.EntityFrameworkCore;
using InventoryService.Data;
using InventoryService.Models;

namespace InventoryService.Services
{
    public class ProductService
    {
        private readonly InventoryDbContext _context;

        public ProductService(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
            => await _context.Products.AsNoTracking().ToListAsync();

        public async Task<Product?> GetByIdAsync(int id)
            => await _context.Products.FindAsync(id);

        public async Task<Product> CreateAsync(Product p)
        {
            _context.Products.Add(p);
            await _context.SaveChangesAsync();
            return p;
        }

        public async Task<bool> UpdateAsync(Product p)
        {
            var existing = await _context.Products.FindAsync(p.Id);
            if (existing == null) return false;

            existing.Name = p.Name;
            existing.Description = p.Description;
            existing.Price = p.Price;
            existing.Stock = p.Stock;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.Products.FindAsync(id);
            if (existing == null) return false;
            _context.Products.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DecreaseStockAsync(int productId, int quantity)
        {
            var p = await _context.Products.FindAsync(productId);
            if (p == null) return false;
            p.Stock -= quantity;
            if (p.Stock < 0) p.Stock = 0; // safeguard
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
