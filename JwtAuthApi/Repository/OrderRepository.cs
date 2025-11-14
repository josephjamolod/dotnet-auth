using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Data;
using JwtAuthApi.Dtos.Orders;
using JwtAuthApi.Helpers.HelperObjects;
using JwtAuthApi.Interfaces;
using JwtAuthApi.Mappers;
using JwtAuthApi.Models;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthApi.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDBContext _context;
        public OrderRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<OperationResult<CheckoutSelectedResponse, ErrorResult>> CheckoutSelectedSellersAsync(CheckoutSelectedRequest request, string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.FoodItem)
                        .ThenInclude(fi => fi.Seller)
                .FirstOrDefaultAsync(c => c.CustomerId == userId);

            if (cart == null || cart.CartItems.Count == 0)
                return OperationResult<CheckoutSelectedResponse, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status400BadRequest,
                    ErrDescription = "Cart is empty"
                });

            // Filter only selected sellers
            var selectedItems = cart.CartItems
                .Where(ci => request.SellerIds.Contains(ci.FoodItem.SellerId))
                .ToList();

            if (selectedItems.Count == 0)
                return OperationResult<CheckoutSelectedResponse, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status400BadRequest,
                    ErrDescription = "No items found for selected sellers"
                });

            // Group by seller
            var sellerGroups = selectedItems
                .GroupBy(ci => ci.FoodItem.SellerId)
                .ToList();

            var createdOrders = new List<OrderDto>();
            var errors = new List<CheckoutError>();

            // Create separate order for each selected seller
            foreach (var sellerGroup in sellerGroups)
            {
                var sellerId = sellerGroup.Key;
                var sellerItems = sellerGroup.ToList();
                var sellerName = sellerItems.First().FoodItem.Seller.BusinessName;

                // Validate seller items
                var validationError = ValidateSellerItems(sellerItems, sellerId, sellerName!);
                if (validationError != null)
                {
                    errors.Add(validationError);
                    continue;
                }

                try
                {
                    // Create order
                    var order = CreateOrderForSeller(sellerItems, userId, sellerId, request);

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    var orderDto = await GetOrderDtoById(order.Id);
                    createdOrders.Add(orderDto!);

                    // Remove items from cart
                    _context.CartItems.RemoveRange(sellerItems);
                }
                catch (Exception ex)
                {
                    errors.Add(new CheckoutError
                    {
                        SellerId = sellerId,
                        SellerName = sellerName,
                        ErrorType = "system_error",
                        Message = $"Failed to create order for {sellerName}: {ex.Message}"
                    });
                }
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var response = new CheckoutSelectedResponse
            {
                Success = createdOrders.Count != 0,
                OrdersCreated = createdOrders.Count,
                TotalOrders = sellerGroups.Count,
                Orders = createdOrders,
                Errors = errors
            };

            if (createdOrders.Count == 0 && errors.Count > 0)
            {
                return OperationResult<CheckoutSelectedResponse, ErrorResult>.Failure(
                    new ErrorResult
                    {
                        ErrCode = StatusCodes.Status400BadRequest,
                        ErrDescription = "Checkout failed for all selected sellers",
                    }
                );
            }

            // CASE 2: Partial success → still Success but includes errors
            if (createdOrders.Count > 0 && errors.Count > 0)
            {
                return OperationResult<CheckoutSelectedResponse, ErrorResult>.Success(response);
            }

            return OperationResult<CheckoutSelectedResponse, ErrorResult>.Success(response);
        }

        public async Task<OperationResult<Order, ErrorResult>> GetOrderByIdAsync(int id)
        {
            try
            {
                var order = await _context.Orders
                     .Include(o => o.Customer)
                     .Include(o => o.Seller)
                     .Include(o => o.OrderItems)
                         .ThenInclude(oi => oi.FoodItem)
                             .ThenInclude(fi => fi.ImageUrls)
                     .FirstOrDefaultAsync(o => o.Id == id);
                if (order == null)
                    return OperationResult<Order, ErrorResult>.Failure(new ErrorResult()
                    {
                        ErrCode = StatusCodes.Status404NotFound,
                        ErrDescription = "Order not found"
                    });

                return OperationResult<Order, ErrorResult>.Success(order);

            }
            catch (Exception)
            {
                return OperationResult<Order, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status500InternalServerError,
                    ErrDescription = "Something Went Wrong Please try again later."
                });
            }
        }

        private static string GenerateOrderNumber()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = new Random().Next(1000, 9999);
            return $"ORD-{date}-{random}";
        }

        private async Task<OrderDto?> GetOrderDtoById(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Seller)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.FoodItem)
                        .ThenInclude(fi => fi.ImageUrls)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
                return null;

            return order.OrderToOrderDto();
        }

        private static CheckoutError? ValidateSellerItems(List<CartItem> sellerItems, string sellerId, string sellerName)
        {
            // Check for unavailable items
            var unavailableItems = sellerItems
                .Where(ci => !ci.FoodItem.IsAvailable)
                .ToList();

            if (unavailableItems.Count != 0)
            {
                return new CheckoutError
                {
                    SellerId = sellerId,
                    SellerName = sellerName,
                    ErrorType = "unavailable_items",
                    Message = $"Some items from {sellerName} are no longer available",
                    Items = unavailableItems.Select(ci => ci.FoodItem.Name).ToList()
                };
            }

            // Check for price changes
            var priceChangedItems = sellerItems
                .Where(ci => ci.PriceSnapshot != ci.FoodItem.Price)
                .ToList();

            if (priceChangedItems.Count != 0)
            {
                return new CheckoutError
                {
                    SellerId = sellerId,
                    SellerName = sellerName,
                    ErrorType = "price_changes",
                    Message = $"Prices have changed for items from {sellerName}",
                    PriceChanges = priceChangedItems.Select(ci => ci.CartItemToPriceChange()).ToList()
                };
            }

            return null;
        }

        private static Order CreateOrderForSeller(List<CartItem> sellerItems, string userId, string sellerId, CheckoutSelectedRequest request)
        {
            // Calculate totals
            var subTotal = sellerItems.Sum(ci => ci.Quantity * ci.FoodItem.Price);
            var maxPrepTime = sellerItems.Max(ci => ci.FoodItem.PreparationTime);
            var estimatedDeliveryTime = maxPrepTime + 30;

            // Create order using shared helper
            var createOrder = new CreateOrderDto()
            {
                UserId = userId,
                SellerId = sellerId,
                SubTotal = subTotal,
                DeliveryFee = request.DeliveryFee,
                EstimatedDeliveryTime = estimatedDeliveryTime,
                DeliveryAddress = request.DeliveryAddress,
                PhoneNumber = request.PhoneNumber,
                Notes = request.Notes
            };

            var order = createOrder.CreateOrderDtoToOrder();
            order.OrderNumber = GenerateOrderNumber();
            // Create order items
            foreach (var cartItem in sellerItems)
            {
                var orderItem = new OrderItem
                {
                    FoodItemId = cartItem.FoodItemId,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.FoodItem.Price,
                    SpecialInstructions = cartItem.SpecialInstructions
                };
                order.OrderItems.Add(orderItem);

                cartItem.FoodItem.TotalSold += cartItem.Quantity;
            }

            return order;
        }
    }
}