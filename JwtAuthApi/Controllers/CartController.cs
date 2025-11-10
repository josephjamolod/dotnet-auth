using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JwtAuthApi.Dtos.Cart;
using JwtAuthApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRepo;
        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        public CartController(ICartRepository cartRepo)
        {
            _cartRepo = cartRepo;
        }
        [HttpPost("add")]
        public async Task<ActionResult<CartItemDto>> AddToCart(AddToCartRequest model)
        {
            try
            {
                var result = await _cartRepo.AddToCartAsync(model, GetUserId());
                if (!result.IsSuccess)
                    return StatusCode(result.Error!.ErrCode, new { message = result.Error.ErrDescription });

                return Ok(result.Value);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error adding item to cart" });
            }
        }

        [HttpGet]
        public async Task<ActionResult<CartResponseDto>> GetCart()
        {
            var result = await _cartRepo.GetCartAsync(GetUserId());
            if (!result.IsSuccess)
                return StatusCode(result.Error!.ErrCode, new { message = result.Error.ErrDescription });

            return Ok(result.Value);
        }
    }
}