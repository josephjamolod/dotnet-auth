using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Dtos.Cart
{
    public class CartResponseDto
    {
        public int CartId { get; set; }
        public List<SellerCartDto> SellerCarts { get; set; } = new();
        public int TotalItems { get; set; }
        public decimal GrandTotal { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastActivityAt { get; set; }
    }
}