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
using JwtAuthApi.Models;
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

        public async Task<OperationResult<object, ErrorResult>> GetTopSellingItemsAsync(int limit, string sellerId)
        {
            try
            {
                // First, get the aggregated data
                var topItemsData = await _context.OrderItems
                         .Include(oi => oi.Order)
                         .Where(oi => oi.FoodItem.SellerId == sellerId &&
                                      oi.Order.Status == OrderStatus.Delivered)
                         .GroupBy(oi => oi.FoodItemId)
                         .Select(g => new
                         {
                             foodItemId = g.Key,
                             totalQuantitySold = g.Sum(oi => oi.Quantity),
                             totalRevenue = g.Sum(oi => oi.Quantity * oi.Price),
                             orderCount = g.Count()
                         })
                         .OrderByDescending(x => x.totalQuantitySold)
                         .Take(limit)
                         .ToListAsync();

                // Get the food item IDs
                var foodItemIds = topItemsData.Select(x => x.foodItemId).ToList();

                // Fetch the food items with images
                var foodItems = await _context.FoodItems
                    .Include(fi => fi.ImageUrls)
                    .Where(fi => foodItemIds.Contains(fi.Id))
                    .ToListAsync();

                // Combine the data
                var topItems = topItemsData.Select(data =>
                {
                    var foodItem = foodItems.First(fi => fi.Id == data.foodItemId);
                    return new
                    {
                        foodItemId = data.foodItemId,
                        name = foodItem.Name,
                        category = foodItem.Category,
                        price = foodItem.Price,
                        imageUrls = foodItem.ImageUrls.Select(img => img.ImageUrl).ToList(),
                        totalQuantitySold = data.totalQuantitySold,
                        totalRevenue = data.totalRevenue,
                        orderCount = data.orderCount
                    };
                }).ToList();

                return OperationResult<object, ErrorResult>.Success(new
                {
                    count = topItems.Count,
                    items = topItems
                });
            }
            catch (Exception)
            {
                return OperationResult<object, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status500InternalServerError,
                    ErrDescription = "Something Went Wrong Retrieving Items"
                });
            }
        }
    }
}