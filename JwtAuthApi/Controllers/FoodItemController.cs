using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JwtAuthApi.Dtos.Foods;
using JwtAuthApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Seller")]
    public class FoodItemController : ControllerBase
    {
        private readonly IFoodItemRepository _foodItemRepo;
        private readonly ILogger<FoodItemController> _logger;
        public FoodItemController(IFoodItemRepository foodItemRepo, ILogger<FoodItemController> logger)
        {
            _foodItemRepo = foodItemRepo;
            _logger = logger;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            try
            {
                var sellerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await _foodItemRepo.FindById(id, sellerId!);
                if (!result.IsSuccess)
                    return NotFound(new { message = result.Error });

                return Ok(result.Value);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again." });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateFoodItemDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var sellerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await _foodItemRepo.CreateAsync(model, sellerId!);
                if (!result.IsSuccess)
                    return NotFound(new { message = result.Error });

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Value!.Id },
                    new
                    {
                        message = "Food item created successfully",
                        id = result.Value.Id,
                        name = result.Value.Name
                    });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again." });
            }
        }
    }
}