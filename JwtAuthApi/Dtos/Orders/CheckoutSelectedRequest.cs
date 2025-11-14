using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Dtos.Orders
{
    public class CheckoutSelectedRequest
    {
        [Required]
        public List<string> SellerIds { get; set; } = new();
        [Required]
        public string DeliveryAddress { get; set; } = string.Empty;
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public decimal? DeliveryFee { get; set; }
    }
}