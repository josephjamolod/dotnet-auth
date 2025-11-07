using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Models;

namespace JwtAuthApi.Dtos.Foods
{
    public class UpdateFoodItemDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        [StringLength(500)]
        public string? Description { get; set; }
        [Required]
        [Range(0.01, 10000)]
        public decimal Price { get; set; }
        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // e.g., "Main Course", "Dessert", "Beverage"
        [Range(1, 180)]
        public int PreparationTime { get; set; } = 30; // In minutes

        public void UpdateFoodItemDtoToFoodItem(FoodItem food)
        {
            food.Name = this.Name ?? food.Name;
            food.Description = this.Description ?? food.Description;
            food.Price = this.Price != default ? this.Price : food.Price;
            food.Category = this.Category ?? food.Category;
            food.PreparationTime = this.PreparationTime != default ? this.PreparationTime : food.PreparationTime;
            food.UpdatedAt = DateTime.UtcNow;
        }
    }
}