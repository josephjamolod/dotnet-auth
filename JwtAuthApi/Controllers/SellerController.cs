using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JwtAuthApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Seller")]
    public class SellerController : ControllerBase
    {
        private readonly ISellerRepository _sellerRepo;
        private readonly ILogger<SellerController> _logger;
        public SellerController(ISellerRepository sellerRepo, ILogger<SellerController> logger)
        {
            _sellerRepo = sellerRepo;
            _logger = logger;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetSellerProfile()
        {
            try
            {
                var sellerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await _sellerRepo.GetSellerProfileAsync(sellerId!);

                if (!result.IsSuccess)
                    return NotFound(result.Error);

                return Ok(new { result.Value });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error retrieving profile" });
            }
        }
    }
}