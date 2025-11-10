using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, 100)] // Prevent unreasonable quantities
        public int Quantity { get; set; }

        [StringLength(200)]
        public string? SpecialInstructions { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Snapshot price at time of adding to cart
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PriceSnapshot { get; set; }

        // Foreign Keys
        [Required]
        public int CartId { get; set; }
        public Cart Cart { get; set; } = null!;

        [Required]
        public int FoodItemId { get; set; }
        public FoodItem FoodItem { get; set; } = null!;

        // Computed property
        [NotMapped]
        public decimal LineTotal => Quantity * PriceSnapshot;
    }
}