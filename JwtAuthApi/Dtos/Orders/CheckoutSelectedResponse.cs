using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Dtos.Orders
{
    public class CheckoutSelectedResponse
    {
        public bool Success { get; set; }
        public int OrdersCreated { get; set; }
        public int TotalOrders { get; set; }
        public List<OrderDto> Orders { get; set; } = new();
        public List<CheckoutError> Errors { get; set; } = new();
    }
}