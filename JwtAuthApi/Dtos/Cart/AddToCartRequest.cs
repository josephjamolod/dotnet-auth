using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Dtos.Cart
{
    public class AddToCartRequest
    {
        public int FoodItemId { get; set; }
        public int Quantity { get; set; } = 1;
        public string? SpecialInstructions { get; set; }
    }
}