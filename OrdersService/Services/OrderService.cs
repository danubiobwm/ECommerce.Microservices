using OrdersService.Data;
using OrdersService.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace OrdersService.Services;

public class OrderService
{
    private readonly OrdersDbContext _db;
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;
    public OrderService(OrdersDbContext db, IHttpClientFactory http, IConfiguration config)
    {
        _db = db; _http = http; _config = config;
    }

    public async Task<(bool Success, string? Message, Order? Order)> CreateOrderAsync(List<(Guid ProductId, int Quantity)> items)
    {
        // 1) check inventory via InventoryService
        var client = _http.CreateClient("inventory");
        var checkObj = new { items = items.Select(i => new { productId = i.ProductId, quantity = i.Quantity }).ToList() };
        var resp = await client.PostAsJsonAsync("/api/products/check", new { items = items.Select(i => new { productId = i.ProductId, quantity = i.Quantity }) });
        if (!resp.IsSuccessStatusCode) return (false, "Falha ao validar estoque", null);
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
        var allAvailable = json.GetProperty("allAvailable").GetBoolean();
        if (!allAvailable)
        {
            return (false, "Produtos indisponíveis", null);
        }

        // 2) create order
        var order = new Order { Id = Guid.NewGuid(), Status = "Confirmed", CreatedAt = DateTime.UtcNow };
        foreach (var it in items)
        {
            order.Items.Add(new OrderItem { Id = Guid.NewGuid(), ProductId = it.ProductId, Quantity = it.Quantity, UnitPrice = 0 });
        }
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        // 3) publish event to RabbitMQ
        PublishOrderCreatedEvent(order);

        return (true, null, order);
    }

    private void PublishOrderCreatedEvent(Order order)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RABBITMQ__HOST"] ?? "rabbitmq",
            UserName = _config["RABBITMQ__USER"] ?? "guest",
            Password = _config["RABBITMQ__PASS"] ?? "guest"
        };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.ExchangeDeclare("orders", ExchangeType.Fanout, durable: true);
        var evt = new
        {
            OrderId = order.Id,
            Items = order.Items.Select(i => new { ProductId = i.ProductId, Quantity = i.Quantity }).ToArray()
        };
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt));
        channel.BasicPublish("orders", "", null, body);
    }
}
