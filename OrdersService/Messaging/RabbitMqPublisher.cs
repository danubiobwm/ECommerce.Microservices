using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace OrdersService.Messaging
{
    public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
    {
        private readonly IConnection _conn;
        private readonly IModel _ch;
        private readonly string _queue = "order_created";

        public RabbitMqPublisher(IConfiguration cfg)
        {
            var factory = new ConnectionFactory
            {
                HostName = cfg["RABBITMQ__HOST"] ?? "rabbitmq",
                UserName = cfg["RABBITMQ__USER"] ?? "guest",
                Password = cfg["RABBITMQ__PASS"] ?? "guest"
            };
            _conn = factory.CreateConnection();
            _ch = _conn.CreateModel();
            _ch.QueueDeclare(_queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        public void PublishOrderItem(int productId, int quantity)
        {
            var obj = new { ProductId = productId, Quantity = quantity };
            var json = JsonSerializer.Serialize(obj);
            var bytes = Encoding.UTF8.GetBytes(json);
            var props = _ch.CreateBasicProperties();
            props.DeliveryMode = 2; // persistent
            _ch.BasicPublish(exchange: "", routingKey: _queue, basicProperties: props, body: bytes);
        }

        public void Dispose()
        {
            _ch?.Close();
            _conn?.Close();
        }
    }
}
