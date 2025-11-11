using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Dtos.Cart;
using JwtAuthApi.Helpers.HelperObjects;

namespace JwtAuthApi.Interfaces
{
    public interface ICartRepository
    {
        Task<OperationResult<CartItemDto, ErrorResult>> AddToCartAsync(AddToCartRequest model, string userId);
        Task<OperationResult<CartResponseDto, ErrorResult>> GetCartAsync(string userId);
        Task<OperationResult<object, ErrorResult>> UpdateCartItemAsync(int cartItemId, string userId, UpdateCartItemRequest request);
        Task<OperationResult<object, ErrorResult>> RemoveFromCartAsync(int cartItemId, string userId);
        Task<OperationResult<object, ErrorResult>> ClearCartAsync(string userId);
        Task<OperationResult<object, ErrorResult>> ClearSellerCartAsync(string sellerId, string userId);
    }
}