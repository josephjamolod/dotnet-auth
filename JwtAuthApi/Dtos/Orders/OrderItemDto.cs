using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Dtos.Orders
{
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int FoodItemId { get; set; }
        public string FoodItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? SpecialInstructions { get; set; }
        public decimal LineTotal { get; set; }
        public string? MainImageUrl { get; set; }
    }
}