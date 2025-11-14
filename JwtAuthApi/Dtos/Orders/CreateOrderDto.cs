using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Dtos.Orders
{
    public class CreateOrderDto
    {
        public string UserId { get; set; } = string.Empty;
        public string SellerId { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal? DeliveryFee { get; set; }
        public int EstimatedDeliveryTime { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}