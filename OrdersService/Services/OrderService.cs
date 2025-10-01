using OrdersService.Models;
using OrdersService.DTOs;

namespace OrdersService.Services
{
    public class OrderService
    {
        private readonly List<Order> _orders = new();
        private int _nextId = 1;

        public async Task<(bool Success, string? Error, OrderResponseDto? Order)> CreateAsync(OrderCreateDto dto)
        {
            var order = new Order
            {
                Id = _nextId++,
                CustomerId = dto.CustomerId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                Items = dto.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            _orders.Add(order);

            return (true, null, MapToResponse(order));
        }

        public async Task<OrderResponseDto?> GetByIdAsync(int id)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
            return order == null ? null : MapToResponse(order);
        }

        public async Task<List<OrderResponseDto>> GetAllAsync()
        {
            return _orders.Select(MapToResponse).ToList();
        }

        private static OrderResponseDto MapToResponse(Order order)
        {
            return new OrderResponseDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                CreatedAt = order.CreatedAt,
                Status = order.Status,
                Items = order.Items.Select(i => new OrderItemResponseDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };
        }
    }
}
