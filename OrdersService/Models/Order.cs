using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OrdersService.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "Pending";

        [JsonIgnore]
        public List<OrderItem> Items { get; set; } = new();
    }
}
