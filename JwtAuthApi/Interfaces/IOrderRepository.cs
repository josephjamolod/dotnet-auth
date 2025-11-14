using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Dtos.Orders;
using JwtAuthApi.Helpers.HelperObjects;

namespace JwtAuthApi.Interfaces
{
    public interface IOrderRepository
    {
        Task<OperationResult<CheckoutSelectedResponse, ErrorResult>> CheckoutSelectedSellersAsync(CheckoutSelectedRequest request, string userId);
    }
}