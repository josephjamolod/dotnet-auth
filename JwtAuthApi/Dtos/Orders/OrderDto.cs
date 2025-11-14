using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Models;

namespace JwtAuthApi.Dtos.Orders
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string SellerId { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public string SellerEmail { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public OrderStatus Status { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Notes { get; set; }
        public int EstimatedDeliveryTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? PreparingAt { get; set; }
        public DateTime? ReadyAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new();
    }
}