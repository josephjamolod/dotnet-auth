using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Dtos.Cart;
using JwtAuthApi.Dtos.Orders;
using JwtAuthApi.Models;

namespace JwtAuthApi.Mappers
{
    public static class CartMappers
    {
        public static CartItemDto CartItemToCartItemDto(this CartItem ci)
        {
            return new CartItemDto()
            {
                Id = ci.Id,
                FoodItemId = ci.FoodItemId,
                FoodItemName = ci.FoodItem.Name,
                FoodItemPrice = ci.FoodItem.Price,
                PriceSnapshot = ci.PriceSnapshot,
                Quantity = ci.Quantity,
                SpecialInstructions = ci.SpecialInstructions,
                AddedAt = ci.AddedAt,
                LineTotal = ci.Quantity * ci.PriceSnapshot,
                IsAvailable = ci.FoodItem.IsAvailable,
                MainImageUrl = ci.FoodItem.ImageUrls.FirstOrDefault(img => img.IsMainImage)?.ImageUrl,
                SellerName = ci.FoodItem.Seller.FirstName
            };
        }

        public static SellerCartDto GroupToSellerCartDto(this IGrouping<string, CartItem> g)
        {
            return new SellerCartDto()
            {
                SellerId = g.Key,
                BusinessName = g.First().FoodItem.Seller.BusinessName,
                SellerRating = g.First().FoodItem.Seller.Rating,
                Items = g.Select(ci => ci.CartItemToCartItemDto()).ToList(),
                SubTotal = g.Sum(ci => ci.Quantity * ci.PriceSnapshot),
                TotalItems = g.Sum(ci => ci.Quantity)
            };
        }

        public static CartResponseDto CartToCartResponseDto(this Cart cart)
        {
            return new CartResponseDto()
            {
                CartId = cart.Id,
                TotalItems = cart.CartItems.Sum(ci => ci.Quantity),
                CreatedAt = cart.CreatedAt,
                LastActivityAt = cart.LastActivityAt
            };
        }

        public static PriceChange CartItemToPriceChange(this CartItem ci)
        {
            return new PriceChange()
            {
                ItemName = ci.FoodItem.Name,
                OldPrice = ci.PriceSnapshot,
                NewPrice = ci.FoodItem.Price
            };
        }
    }
}