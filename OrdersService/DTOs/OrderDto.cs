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
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
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
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = null!;
        public List<OrderItemDto> Items { get; set; } = new();
    }
}
