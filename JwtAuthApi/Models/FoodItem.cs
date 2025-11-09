using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Models
{
    public class FoodItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        [StringLength(500)]
        public string? Description { get; set; }
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // e.g., "Main Course", "Dessert", "Beverage"
        public bool IsAvailable { get; set; } = true;
        public int PreparationTime { get; set; } = 30; // In minutes
        public decimal Rating { get; set; } = 0;
        public int TotalRatings { get; set; } = 0;
        public int TotalSold { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign Key
        [Required]
        public string SellerId { get; set; } = string.Empty;
        public AppUser Seller { get; set; } = null!;

        // Navigation properties
        public List<OrderItem> OrderItems { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
        public List<FoodImage> ImageUrls { get; set; } = new();
        public List<CartItem> CartItems { get; set; } = new();
    }
}