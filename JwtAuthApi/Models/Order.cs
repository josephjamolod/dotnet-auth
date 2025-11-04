using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Models
{
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Preparing,
        Ready,
        OutForDelivery,
        Delivered,
        Cancelled
    }
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string OrderNumber { get; set; } = string.Empty; // e.g., "ORD-20231030-0001"

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DeliveryFee { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Tax { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        [StringLength(500)]
        public string DeliveryAddress { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? PreparingAt { get; set; }
        public DateTime? ReadyAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        public int EstimatedDeliveryTime { get; set; } // In minutes

        // Foreign Key that connect each Order to a customer
        [Required]
        public string CustomerId { get; set; } = string.Empty;
        public AppUser Customer { get; set; } = null!;
        // Foreign Key that connect each Order to a Seller
        [Required]
        public string SellerId { get; set; } = string.Empty;
        public AppUser Seller { get; set; } = null!;

    }
}