using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Dtos.Orders
{
    public class BuyNowRequest
    {
        [Required]
        public int FoodItemId { get; set; }
        [Required]
        public int Quantity { get; set; } = 1;
        [Required]
        [StringLength(500)]
        public string DeliveryAddress { get; set; } = string.Empty;
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? SpecialInstructions { get; set; }
        public decimal? DeliveryFee { get; set; }
    }
}