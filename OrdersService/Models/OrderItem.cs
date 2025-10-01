using System.Text.Json.Serialization;

namespace OrdersService.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public int OrderId { get; set; }

        [JsonIgnore]
        public Order? Order { get; set; }
    }
}
