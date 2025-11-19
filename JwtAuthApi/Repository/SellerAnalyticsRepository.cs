using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Data;
using JwtAuthApi.Dtos.Orders;
using JwtAuthApi.Helpers.HelperObjects;
using JwtAuthApi.Helpers.QueryBuilders;
using JwtAuthApi.Interfaces;
using JwtAuthApi.Mappers;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthApi.Repository
{
    public class SellerAnalyticsRepository : ISellerAnalyticsRepository
    {
        private readonly ApplicationDBContext _context;
        public SellerAnalyticsRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<OperationResult<PaginatedResponse<OrderDto>, ErrorResult>> GetSellerOrdersAsync(MyOrdersQuery queryObject, string sellerId)
        {
            try
            {
                var query = _context.Orders
                   .Include(o => o.Customer)
                   .Include(o => o.Seller)
                   .Include(o => o.OrderItems)
                       .ThenInclude(oi => oi.FoodItem)
                           .ThenInclude(fi => fi.ImageUrls)
                   .Where(o => o.SellerId == sellerId);

                // Apply filters
                query = UserOrderQueryBuilder.ApplyFilters(query, queryObject);
                // Apply sorting
                query = UserOrderQueryBuilder.ApplySorting(query, queryObject);

                // Get total count before pagination
                var totalCount = await query.CountAsync();
                // Apply pagination
                var skip = (queryObject.PageNumber - 1) * queryObject.PageSize;

                //  Materialize the data from database
                var ordersFromDb = await query
                    .Skip(skip)
                    .Take(queryObject.PageSize)
                    .ToListAsync();
                //  Apply the mapper in-memory
                var orders = ordersFromDb
                    .Select(o => o.OrderToOrderDto())
                    .ToList();

                return OperationResult<PaginatedResponse<OrderDto>, ErrorResult>.Success(new PaginatedResponse<OrderDto>()
                {
                    Total = totalCount,
                    PageNumber = queryObject.PageNumber,
                    PageSize = queryObject.PageSize,
                    Items = orders
                });
            }
            catch (Exception)
            {
                return OperationResult<PaginatedResponse<OrderDto>, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status500InternalServerError,
                    ErrDescription = "Something Went Wrong Retrieving Orders"
                });
            }
        }
    }
}