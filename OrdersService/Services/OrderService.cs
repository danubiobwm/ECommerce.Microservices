using Microsoft.EntityFrameworkCore;
using OrdersService.Data;
using OrdersService.DTOs;
using OrdersService.Models;

namespace OrdersService.Services
{
    public class OrderService
    {
        private readonly OrdersDbContext _db;
        private readonly ILogger<OrderService> _logger;

        public OrderService(OrdersDbContext db, ILogger<OrderService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<(bool Success, string? Error, OrderResponseDto? Order)> CreateAsync(OrderCreateDto dto)
        {
            try
            {
                var order = new Order
                {
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

                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                return (true, null, MapToDto(order));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return (false, ex.Message, null);
            }
        }

        public async Task<List<OrderResponseDto>> GetAllAsync()
        {
            var orders = await _db.Orders.Include(o => o.Items).ToListAsync();
            return orders.Select(MapToDto).ToList();
        }

        public async Task<OrderResponseDto?> GetByIdAsync(int id)
        {
            var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            return order == null ? null : MapToDto(order);
        }

        private static OrderResponseDto MapToDto(Order order)
        {
            return new OrderResponseDto
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                Status = order.Status,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };
        }
    }
}
