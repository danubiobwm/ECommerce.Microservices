namespace InventoryService.Messaging
{
    public class OrderItemMessage
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}