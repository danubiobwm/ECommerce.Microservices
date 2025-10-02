using Microsoft.EntityFrameworkCore;
using OrdersService.Data;
using OrdersService.DTOs;
using OrdersService.Models;

namespace OrdersService.Services
{
    public class OrderService
    {
        private readonly OrdersDbContext _db;

        public OrderService(OrdersDbContext db)
        {
            _db = db;
        }

        public async Task<OrderResponseDto> CreateAsync(OrderCreateDto dto)
        {
            var order = new Order
            {
                CustomerId = dto.CustomerId,
                CreatedAt = DateTime.UtcNow,
                Status = "Pending",
                Items = dto.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            return MapToResponse(order);
        }

        public async Task<List<OrderResponseDto>> GetAllAsync()
        {
            var orders = await _db.Orders
                .Include(o => o.Items)
                .AsNoTracking()
                .ToListAsync();

            return orders.Select(MapToResponse).ToList();
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
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };
        }
    }
}
