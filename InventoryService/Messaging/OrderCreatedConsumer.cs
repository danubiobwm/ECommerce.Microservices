using InventoryService.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace InventoryService.Messaging;

public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}
public class OrderItem { public Guid ProductId { get; set; } public int Quantity { get; set; } }

public class OrderCreatedConsumer : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly IServiceProvider _sp;
    private RabbitMQ.Client.IConnection? _connection;
    private RabbitMQ.Client.IModel? _channel;

    public OrderCreatedConsumer(IConfiguration config, ILogger<OrderCreatedConsumer> logger, IServiceProvider sp)
    {
        _config = config; _logger = logger; _sp = sp;

        var factory = new ConnectionFactory
        {
            HostName = _config["RABBITMQ__HOST"] ?? "rabbitmq",
            UserName = _config["RABBITMQ__USER"] ?? "guest",
            Password = _config["RABBITMQ__PASS"] ?? "guest",
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(exchange: "orders", type: ExchangeType.Fanout, durable: true);
        _channel.QueueDeclare(queue: "inventory.order.created", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind("inventory.order.created", "orders", "");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var ev = JsonSerializer.Deserialize<OrderCreatedEvent>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (ev != null)
                {
                    using var scope = _sp.CreateScope();
                    var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
                    foreach (var item in ev.Items)
                    {
                        var ok = await productService.ReduceStock(item.ProductId, item.Quantity);
                        if (!ok) _logger.LogWarning("Falha ao reduzir estoque do produto {id}", item.ProductId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro processando evento order.created");
            }
            finally
            {
                _channel?.BasicAck(ea.DeliveryTag, false);
            }
        };

        _channel.BasicConsume(queue: "inventory.order.created", autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
