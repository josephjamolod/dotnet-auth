using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Dtos.Cart
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int FoodItemId { get; set; }
        public string FoodItemName { get; set; } = string.Empty;
        public decimal FoodItemPrice { get; set; }
        public decimal PriceSnapshot { get; set; }
        public int Quantity { get; set; }
        public string? SpecialInstructions { get; set; }
        public DateTime AddedAt { get; set; }
        public decimal LineTotal { get; set; }
        public bool IsAvailable { get; set; }
        public string? MainImageUrl { get; set; }
        public string? SellerName { get; set; }
    }
}