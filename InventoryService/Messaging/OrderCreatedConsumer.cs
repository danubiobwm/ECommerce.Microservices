using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using InventoryService.Services;
using InventoryService.Messaging;

namespace InventoryService.Messaging
{
    public class OrderCreatedConsumer : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _queueName = "order_created";

        public OrderCreatedConsumer(IServiceProvider sp, IConfiguration configuration)
        {
            _sp = sp;
            var factory = new ConnectionFactory()
            {
                HostName = configuration["RABBITMQ__HOST"] ?? "rabbitmq",
                UserName = configuration["RABBITMQ__USER"] ?? "guest",
                Password = configuration["RABBITMQ__PASS"] ?? "guest",
                DispatchConsumersAsync = true
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var bytes = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(bytes);
                    var msg = JsonSerializer.Deserialize<OrderItemMessage>(json);
                    if (msg != null)
                    {
                        using var scope = _sp.CreateScope();
                        var svc = scope.ServiceProvider.GetRequiredService<ProductService>();
                        await svc.DecreaseStockAsync(msg.ProductId, msg.Quantity);
                    }
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch
                {
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                }
            };

            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
