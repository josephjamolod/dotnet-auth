using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Dtos.Cart
{
    public class SellerCartDto
    {
        public string SellerId { get; set; } = string.Empty;
        public string? BusinessName { get; set; }
        public decimal SellerRating { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public decimal SubTotal { get; set; }
        public int TotalItems { get; set; }
    }
}