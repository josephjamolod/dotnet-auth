using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Dtos.Orders;
using JwtAuthApi.Helpers.HelperObjects;
using JwtAuthApi.Models;

namespace JwtAuthApi.Interfaces
{
    public interface IOrderRepository
    {
        Task<OperationResult<CheckoutSelectedResponse, ErrorResult>> CheckoutSelectedSellersAsync(CheckoutSelectedRequest request, string userId);
        Task<OperationResult<Order, ErrorResult>> GetOrderByIdAsync(int id);
    }
}