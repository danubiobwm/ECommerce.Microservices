namespace InventoryService.DTOs;
public record ProductCreateDto(string Name, string Description, decimal Price, int Quantity);
public record ProductDto(Guid Id, string Name, string Description, decimal Price, int Quantity);
public record CheckItemDto(Guid ProductId, int Quantity);
public record CheckAvailabilityRequest(List<CheckItemDto> Items);
public record CheckAvailabilityResponse(bool AllAvailable, List<CheckItemDto> UnavailableItems);
