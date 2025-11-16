using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Models;

namespace JwtAuthApi.Helpers.HelperObjects
{
    public class MyOrdersQuery
    {
        public string? OrderNumber { get; set; } = null;
        [Column(TypeName = "decimal(10,2)")]
        public int Total { get; set; }

        public OrderStatus? Status { get; set; } = OrderStatus.Pending;
        public bool IsDescending { get; set; } = false;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}