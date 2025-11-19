using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Dtos.Orders;
using JwtAuthApi.Helpers.HelperObjects;

namespace JwtAuthApi.Interfaces
{
    public interface ISellerAnalyticsRepository
    {
        Task<OperationResult<PaginatedResponse<OrderDto>, ErrorResult>> GetSellerOrdersAsync(MyOrdersQuery queryObject, string sellerId);
        Task<OperationResult<object, ErrorResult>> GetTopSellingItemsAsync(int limit, string sellerId);
    }
}