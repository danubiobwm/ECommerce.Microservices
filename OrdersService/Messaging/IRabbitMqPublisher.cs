namespace OrdersService.Messaging
{
    public interface IRabbitMqPublisher
    {
        void PublishOrderItem(int productId, int quantity);
    }
}
