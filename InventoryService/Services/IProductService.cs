using InventoryService.Models;

namespace InventoryService.Services;

public interface IProductService
{
    Task<Product> Create(Product p);
    Task<List<Product>> GetAll();
    Task<Product?> Get(Guid id);
    Task<bool> ReduceStock(Guid productId, int qty);
    Task<CheckResult> CheckAvailability(List<CheckItem> items);
}

public record CheckItem(Guid ProductId, int Quantity);
public record CheckResult(bool AllAvailable, List<CheckItem> UnavailableItems);
