using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Dtos.Cart
{
    public class ValidationIssue
    {
        public string Type { get; set; } = string.Empty;  // "unavailable", "priceIncrease", "priceDecrease"
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal? OldPrice { get; set; }
        public decimal? NewPrice { get; set; }
        public decimal? PriceDifference { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}