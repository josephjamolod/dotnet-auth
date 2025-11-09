using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Data;
using JwtAuthApi.Dtos.Cart;
using JwtAuthApi.Helpers.HelperObjects;
using JwtAuthApi.Interfaces;
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
            // return NotFound(new { message = "Food item not found" });

            if (!foodItem.IsAvailable)
                return OperationResult<CartItemDto, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status400BadRequest,
                    ErrDescription = "This item is currently unavailable"
                });
            // return BadRequest(new { message = "This item is currently unavailable" });

            // Get or create cart
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == userId);

            if (cart == null)
            {
                cart = new Cart { CustomerId = userId };
                _context.Carts.Add(cart);
            }

            // Check if cart has items from different seller
            if (cart.SellerId != null && cart.SellerId != foodItem.SellerId)
                return OperationResult<CartItemDto, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status400BadRequest,
                    ErrDescription = "This item is currently unavailable"
                });
            // return BadRequest(new { message = "You can only order from one seller at a time. Please clear your cart or checkout first." });


            // Set seller if first item
            if (cart.SellerId == null)
                cart.SellerId = foodItem.SellerId;

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
    }
}