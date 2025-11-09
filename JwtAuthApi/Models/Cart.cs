using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Useful for cart expiration/cleanup
        public DateTime? LastActivityAt { get; set; }

        // Optional Foreign Key: Store selected seller to ensure cart items are from same seller
        public string? SellerId { get; set; }
        public AppUser? Seller { get; set; }

        // Foreign Key
        [Required]
        public string CustomerId { get; set; } = string.Empty;
        public AppUser Customer { get; set; } = null!;

        // Navigation property
        public List<CartItem> CartItems { get; set; } = new();

        // Computed properties (not stored in DB)
        [NotMapped]
        public decimal SubTotal => CartItems.Sum(ci => ci.Quantity * ci.FoodItem.Price);

        [NotMapped]
        public int TotalItems => CartItems.Sum(ci => ci.Quantity);
    }
}