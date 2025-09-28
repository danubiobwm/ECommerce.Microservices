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

public class OrderItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class OrderCreatedConsumer : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly IServiceProvider _sp;
    private IConnection? _connection;
    private IModel? _channel;

    public OrderCreatedConsumer(IConfiguration config, ILogger<OrderCreatedConsumer> logger, IServiceProvider sp)
    {
        _config = config;
        _logger = logger;
        _sp = sp;

        try
        {
            var factory = new ConnectionFactory()
            {
                HostName = _config["RABBITMQ__HOST"] ?? "rabbitmq",
                UserName = _config["RABBITMQ__USER"] ?? "guest",
                Password = _config["RABBITMQ__PASS"] ?? "guest",
                DispatchConsumersAsync = true  // Disponível na 7.1.2
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: "orders", type: ExchangeType.Fanout, durable: true);
            _channel.QueueDeclare(queue: "inventory.order.created", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind("inventory.order.created", "orders", routingKey: "");

            _logger.LogInformation("RabbitMQ consumer conectado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao conectar com RabbitMQ");
            throw;
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null)
        {
            _logger.LogError("Canal RabbitMQ não inicializado");
            return Task.CompletedTask;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var deliveryTag = ea.DeliveryTag;

            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var ev = JsonSerializer.Deserialize<OrderCreatedEvent>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (ev != null && ev.Items.Any())
                {
                    using var scope = _sp.CreateScope();
                    var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

                    foreach (var item in ev.Items)
                    {
                        var ok = await productService.ReduceStock(item.ProductId, item.Quantity);
                        if (!ok)
                        {
                            _logger.LogWarning("Falha ao reduzir estoque do produto {ProductId}", item.ProductId);
                        }
                        else
                        {
                            _logger.LogInformation("Estoque reduzido para produto {ProductId}, quantidade: {Quantity}",
                                item.ProductId, item.Quantity);
                        }
                    }

                    _logger.LogInformation("Evento processado com sucesso: {OrderId}", ev.OrderId);
                }

                _channel.BasicAck(deliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro processando evento order.created");
                // Rejeita a mensagem para evitar loop infinito
                _channel.BasicNack(deliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(queue: "inventory.order.created", autoAck: false, consumer: consumer);

        _logger.LogInformation("Consumer iniciado para a fila: inventory.order.created");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Parando RabbitMQ consumer...");

        _channel?.Close();
        _connection?.Close();

        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}