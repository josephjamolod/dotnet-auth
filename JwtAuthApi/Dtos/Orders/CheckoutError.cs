using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Dtos.Orders
{
    public class CheckoutError
    {
        public string SellerId { get; set; } = string.Empty;
        public string? SellerName { get; set; }
        public string ErrorType { get; set; } = string.Empty; // "unavailable_items", "price_changes", "system_error"
        public string Message { get; set; } = string.Empty;
        public List<string>? Items { get; set; }
        public List<PriceChange>? PriceChanges { get; set; }
    }
}