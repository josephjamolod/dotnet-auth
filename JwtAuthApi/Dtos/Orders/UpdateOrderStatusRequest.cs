using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Models;

namespace JwtAuthApi.Dtos.Orders
{
    public class UpdateOrderStatusRequest
    {
        public OrderStatus Status { get; set; }
    }

    public class UpdateOrderStatusParams
    {
        public int OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public string UserId { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public bool IsSeller { get; set; }
    }
}