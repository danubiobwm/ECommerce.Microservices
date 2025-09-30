namespace OrdersService.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string? CustomerId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // adicionado Status para controlar fluxo
        public string Status { get; set; } = "Pending";

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }
    }
}
