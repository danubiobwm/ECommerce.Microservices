using OrdersService.Models;

namespace OrdersService.Services;

public interface IOrderService
{
    Task<Order> CreateAsync(OrderCreateRequest req);
    Task<Order?> GetByIdAsync(int id);
}

public record OrderCreateRequest(List<OrderItemRequest> Items);
public record OrderItemRequest(int ProductId, int Quantity);
