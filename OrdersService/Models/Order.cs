using System;
using System.Collections.Generic;

namespace OrdersService.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";

        public List<OrderItem> Items { get; set; } = new();
    }
}
