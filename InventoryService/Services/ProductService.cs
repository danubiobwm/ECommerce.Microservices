using InventoryService.Data;
using InventoryService.Models;
using InventoryService.Services;

public class ProductService : IProductService
{
    private readonly InventoryDbContext _db;
    public ProductService(InventoryDbContext db) { _db = db; }

    public async Task<Product> Create(Product p)
    {
        p.Id = Guid.NewGuid();
        _db.Products.Add(p);
        await _db.SaveChangesAsync();
        return p;
    }
    public Task<List<Product>> GetAll() => _db.Products.ToListAsync();
    public Task<Product?> Get(Guid id) => _db.Products.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<bool> ReduceStock(Guid productId, int qty)
    {
        var product = await _db.Products.FindAsync(productId);
        if (product == null || product.Quantity < qty) return false;
        product.Quantity -= qty;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<CheckResult> CheckAvailability(List<CheckItem> items)
    {
        var unavailable = new List<CheckItem>();
        foreach (var it in items)
        {
            var p = await _db.Products.FindAsync(it.ProductId);
            if (p == null || p.Quantity < it.Quantity) unavailable.Add(it);
        }
        return new CheckResult(unavailable.Count == 0, unavailable);
    }
}