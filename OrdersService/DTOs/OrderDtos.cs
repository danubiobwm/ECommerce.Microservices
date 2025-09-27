namespace OrdersService.DTOs;
public record OrderItemDto(Guid ProductId, int Quantity);
public record CreateOrderDto(List<OrderItemDto> Items);
public record OrderDto(Guid Id, DateTime CreatedAt, string Status, List<OrderItemDto> Items);
