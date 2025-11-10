using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}