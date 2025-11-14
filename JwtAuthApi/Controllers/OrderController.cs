using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JwtAuthApi.Dtos.Orders;
using JwtAuthApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepo;
        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        public OrderController(IOrderRepository orderRepo)
        {
            _orderRepo = orderRepo;
        }
        [HttpPost("checkout-selected")]
        public async Task<ActionResult<CheckoutSelectedResponse>> CheckoutSelectedSellers(CheckoutSelectedRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            // Call repository method
            var result = await _orderRepo.CheckoutSelectedSellersAsync(request, GetUserId());

            if (!result.IsSuccess)
            {
                // All sellers failed
                return BadRequest(result.Error);
            }

            // Partial success or full success
            var response = result.Value!; // guaranteed to be non-null if IsSuccess == true

            if (response.Orders.Count > 0 && response.Errors.Count > 0)
            {
                // Partial success: some orders created, some errors
                return StatusCode(207, response); // Multi-status
            }

            // Full success: all orders created, no errors
            return Ok(response);
        }
    }
}