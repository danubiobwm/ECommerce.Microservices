using InventoryService.Models;

namespace InventoryService.Services;

public interface IProductService
{
    Task<Product> CreateAsync(Product p);
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetAsync(int id);
    Task<bool> ReduceStock(int productId, int qty);
    Task<CheckResult> CheckAvailability(List<CheckItem> items);
}

public record CheckItem(int ProductId, int Quantity);
public record CheckResult(bool AllAvailable, List<CheckItem> UnavailableItems);
