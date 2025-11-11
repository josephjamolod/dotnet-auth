using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JwtAuthApi.Data;
using JwtAuthApi.Dtos.Cart;
using JwtAuthApi.Helpers.HelperObjects;
using JwtAuthApi.Interfaces;
using JwtAuthApi.Mappers;
using JwtAuthApi.Models;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthApi.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDBContext _context;
        public CartRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<OperationResult<CartItemDto, ErrorResult>> AddToCartAsync(AddToCartRequest model, string userId)
        {
            // Validate food item exists and is available
            var foodItem = await _context.FoodItems
                .Include(f => f.Seller)
                .Include(f => f.ImageUrls)
                .FirstOrDefaultAsync(f => f.Id == model.FoodItemId);

            if (foodItem == null)
                return OperationResult<CartItemDto, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status404NotFound,
                    ErrDescription = "Food item not found"
                });

            if (!foodItem.IsAvailable)
                return OperationResult<CartItemDto, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status400BadRequest,
                    ErrDescription = "This item is currently unavailable"
                });

            // Get or create cart
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == userId);

            if (cart == null)
            {
                cart = new Cart { CustomerId = userId };
                _context.Carts.Add(cart);
            }

            // Check if item already in cart
            var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.FoodItemId == model.FoodItemId);

            if (existingCartItem != null)
            {
                // Update quantity
                existingCartItem.Quantity += model.Quantity;
                existingCartItem.UpdatedAt = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(model.SpecialInstructions))
                {
                    existingCartItem.SpecialInstructions = model.SpecialInstructions;
                }
            }
            else
            {
                // Add new cart item
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    FoodItemId = model.FoodItemId,
                    Quantity = model.Quantity,
                    SpecialInstructions = model.SpecialInstructions,
                    PriceSnapshot = foodItem.Price
                };
                cart.CartItems.Add(cartItem);
            }

            cart.LastActivityAt = DateTime.UtcNow;
            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var resultItem = existingCartItem ?? cart.CartItems.Last();

            return OperationResult<CartItemDto, ErrorResult>.Success(new CartItemDto
            {
                Id = resultItem.Id,
                FoodItemId = resultItem.FoodItemId,
                FoodItemName = foodItem.Name,
                FoodItemPrice = foodItem.Price,
                PriceSnapshot = resultItem.PriceSnapshot,
                Quantity = resultItem.Quantity,
                SpecialInstructions = resultItem.SpecialInstructions,
                AddedAt = resultItem.AddedAt,
                LineTotal = resultItem.Quantity * resultItem.PriceSnapshot,
                IsAvailable = foodItem.IsAvailable,
                MainImageUrl = foodItem.ImageUrls.FirstOrDefault(img => img.IsMainImage)?.ImageUrl,
                SellerName = foodItem.Seller.BusinessName
            });
        }

        public async Task<OperationResult<CartResponseDto, ErrorResult>> GetCartAsync(string userId)
        {
            try
            {
                var cart = await _context.Carts
                       .Include(c => c.CartItems)
                           .ThenInclude(ci => ci.FoodItem)
                               .ThenInclude(fi => fi.ImageUrls)
                       .Include(c => c.CartItems)
                           .ThenInclude(ci => ci.FoodItem)
                               .ThenInclude(fi => fi.Seller)
                       .FirstOrDefaultAsync(c => c.CustomerId == userId);

                if (cart == null)
                {
                    // Create empty cart if doesn't exist
                    cart = new Cart { CustomerId = userId };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                }

                // Group cart items by seller
                var sellerGroups = cart.CartItems
                    .GroupBy(ci => ci.FoodItem.SellerId)
                    .Select(g => g.GroupToSellerCartDto()).ToList();

                var cartResponse = cart.CartToCartResponseDto();
                cartResponse.SellerCarts = sellerGroups;
                cartResponse.GrandTotal = sellerGroups.Sum(s => s.SubTotal);
                return OperationResult<CartResponseDto, ErrorResult>.Success(cartResponse);
            }
            catch (Exception)
            {
                return OperationResult<CartResponseDto, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status500InternalServerError,
                    ErrDescription = "Something went wrong, please try again"
                });
            }
        }

        public async Task<OperationResult<object, ErrorResult>> UpdateCartItemAsync(int cartItemId, string userId, UpdateCartItemRequest request)
        {
            try
            {
                var cartItem = await _context.CartItems
                  .Include(ci => ci.Cart)
                  .Include(ci => ci.FoodItem)
                  .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.CustomerId == userId);

                if (cartItem == null)
                    return OperationResult<object, ErrorResult>.Failure(new ErrorResult()
                    {
                        ErrCode = StatusCodes.Status404NotFound,
                        ErrDescription = "Cart item not found"
                    });
                // return NotFound(new { message = "Cart item not found" });

                if (request.Quantity <= 0)
                    return OperationResult<object, ErrorResult>.Failure(new ErrorResult()
                    {
                        ErrCode = StatusCodes.Status400BadRequest,
                        ErrDescription = "Quantity must be greater than 0"
                    });
                // return BadRequest(new { message = "Quantity must be greater than 0" });

                cartItem.Quantity = request.Quantity;
                if (request.SpecialInstructions != null)
                {
                    cartItem.SpecialInstructions = request.SpecialInstructions;
                }
                cartItem.UpdatedAt = DateTime.UtcNow;
                cartItem.Cart.LastActivityAt = DateTime.UtcNow;
                cartItem.Cart.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return OperationResult<object, ErrorResult>.Success(new { message = "Cart item updated successfully" });
            }
            catch (Exception)
            {
                return OperationResult<object, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status500InternalServerError,
                    ErrDescription = "Something went wrong adding item quantity"
                });
            }
        }

        public async Task<OperationResult<object, ErrorResult>> RemoveItemFromCartAsync(int cartItemId, string userId)
        {
            try
            {
                var cartItem = await _context.CartItems
                        .Include(ci => ci.Cart)
                        .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.CustomerId == userId);

                if (cartItem == null)
                    return OperationResult<object, ErrorResult>.Failure(new ErrorResult()
                    {
                        ErrCode = StatusCodes.Status404NotFound,
                        ErrDescription = "Cart item not found"
                    });

                var cart = cartItem.Cart;
                _context.CartItems.Remove(cartItem);

                cart.LastActivityAt = DateTime.UtcNow;
                cart.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return OperationResult<object, ErrorResult>.Success(new { message = "Item removed from cart" });
            }
            catch (Exception)
            {
                return OperationResult<object, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status500InternalServerError,
                    ErrDescription = "Something went wrong removing item"
                });
            }
        }

        public async Task<OperationResult<object, ErrorResult>> ClearCartAsync(string userId)
        {
            try
            {
                var cart = await _context.Carts
                  .Include(c => c.CartItems)
                  .FirstOrDefaultAsync(c => c.CustomerId == userId);

                if (cart == null)
                    return OperationResult<object, ErrorResult>.Failure(new ErrorResult()
                    {
                        ErrCode = StatusCodes.Status404NotFound,
                        ErrDescription = "Cart not found"
                    });

                _context.CartItems.RemoveRange(cart.CartItems);
                cart.LastActivityAt = DateTime.UtcNow;
                cart.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return OperationResult<object, ErrorResult>.Success(new { message = "Cart cleared successfully" });
            }
            catch (Exception)
            {
                return OperationResult<object, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status500InternalServerError,
                    ErrDescription = "Something went wrong removing cart items"
                });
            }
        }

        public async Task<OperationResult<object, ErrorResult>> ClearSellerCartAsync(string sellerId, string userId)
        {
            try
            {
                var cart = await _context.Carts
                       .Include(c => c.CartItems)
                           .ThenInclude(ci => ci.FoodItem)
                       .FirstOrDefaultAsync(c => c.CustomerId == userId);

                if (cart == null)
                    return OperationResult<object, ErrorResult>.Failure(new ErrorResult()
                    {
                        ErrCode = StatusCodes.Status404NotFound,
                        ErrDescription = "Cart not found"
                    });

                // Get items from specific seller
                var sellerItems = cart.CartItems
                    .Where(ci => ci.FoodItem.SellerId == sellerId)
                    .ToList();

                if (!sellerItems.Any())
                    return OperationResult<object, ErrorResult>.Failure(new ErrorResult()
                    {
                        ErrCode = StatusCodes.Status404NotFound,
                        ErrDescription = "No items from this seller in cart"
                    });

                _context.CartItems.RemoveRange(sellerItems);
                cart.LastActivityAt = DateTime.UtcNow;
                cart.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return OperationResult<object, ErrorResult>.Success(new { message = $"Removed {sellerItems.Count} items from cart" });
            }
            catch (Exception)
            {
                return OperationResult<object, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status500InternalServerError,
                    ErrDescription = "Something went wrong removing cart items"
                });
            }
        }
    }
}