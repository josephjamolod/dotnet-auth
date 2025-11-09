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
    }
}