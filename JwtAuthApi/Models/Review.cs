using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key that connect each Review to a Customer
        [Required]
        public string CustomerId { get; set; } = string.Empty;
        public AppUser Customer { get; set; } = null!;

        // Foreign Key that connect each Review to a Seller
        [Required]
        public string SellerId { get; set; } = string.Empty;
        public AppUser Seller { get; set; } = null!;

        // Foreign Key that connect each Review to a FoodItem
        public int? FoodItemId { get; set; } // Nullable: can review seller or specific item
        public FoodItem? FoodItem { get; set; }

        // Foreign Key that connect each a Review to a FoodItem (1to1 Relationship)
        [Required]
        public int OrderId { get; set; }

        public Order Order { get; set; } = null!;



    }
}