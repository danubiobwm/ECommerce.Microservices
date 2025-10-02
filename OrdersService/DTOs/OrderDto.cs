using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrdersService.DTOs
{

    public class OrderCreateDto
    {
        [Required]
        public string CustomerId { get; set; } = null!;

        [Required]
        [MinLength(1, ErrorMessage = "É necessário pelo menos 1 item no pedido.")]
        public List<OrderItemCreateDto> Items { get; set; } = new();
    }

    public class OrderItemCreateDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity deve ser >= 1")]
        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
    }

    public class OrderResponseDto
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "Pending";
        public List<OrderItemResponseDto> Items { get; set; } = new();
    }

    public class OrderItemResponseDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
