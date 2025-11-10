using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Dtos.Cart
{
    public class UpdateCartItemRequest
    {
        public int Quantity { get; set; }
        public string? SpecialInstructions { get; set; }
    }
}