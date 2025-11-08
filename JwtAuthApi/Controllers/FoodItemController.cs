using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JwtAuthApi.Data;
using JwtAuthApi.Dtos.Foods;
using JwtAuthApi.Helpers.HelperObjects;
using JwtAuthApi.Interfaces;
using JwtAuthApi.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet]
        public async Task<IActionResult> GetAllFoodItems([FromQuery] AllFoodsQuery queryObject)
        {
            try
            {
                var sellerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await _foodItemRepo.GetAllFoodItemsAsync(queryObject, sellerId!);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error retrieving food items" });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            try
            {
                var sellerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await _foodItemRepo.GetByIdAsync(id, sellerId!);
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
        [HttpPost("{id}/images")]
        public async Task<IActionResult> UploadFoodImages(
           int id,
           [FromForm] List<IFormFile> images,
           [FromForm] bool setFirstAsMain = true)
        {
            if (images == null || images.Count != 0)
                return BadRequest(new { message = "No images uploaded" });

            // Validate max 5 images
            if (images.Count > 5)
                return BadRequest(new { message = "Maximum 5 images allowed" });
            try
            {
                var sellerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await _foodItemRepo.UploadFoodImagesAsync(id, images, setFirstAsMain, sellerId!);
                if (!result.IsSuccess)
                    return StatusCode(result.Error!.ErrCode, new { message = result.Error.ErrDescription });
                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading images for item {id}");
                return StatusCode(500, new { message = "Error uploading images" });
            }
        }

        [HttpDelete("images/{imageId}")]
        public async Task<IActionResult> DeleteFoodImage(int imageId)
        {
            try
            {
                var sellerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await _foodItemRepo.DeleteFoodImageAsync(imageId, sellerId!);

                if (!result.IsSuccess)
                    return NotFound(new { message = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting image {imageId}");
                return StatusCode(500, new { message = "Error deleting image" });
            }
        }

        [HttpPatch("images/{imageId}/set-main")]
        public async Task<IActionResult> SetMainFoodImage(int imageId)
        {
            try
            {
                var sellerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var result = await _foodItemRepo.SetMainFoodImageAsync(imageId, sellerId!);

                if (!result.IsSuccess)
                    return NotFound(new { message = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting main image {imageId}");
                return StatusCode(500, new { message = "Error updating main image" });
            }
        }

        [HttpPatch("{id}/availability")]
        public async Task<IActionResult> SetAvailability(int id, [FromBody] bool isAvailable)
        {
            try
            {
                var sellerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var result = await _foodItemRepo.SetAvailabilityAsync(id, isAvailable, sellerId!);

                if (!result.IsSuccess)
                    return NotFound(new { message = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting availability for item {id}");
                return StatusCode(500, new { message = "Error updating availability" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFoodItem(int id, [FromBody] UpdateFoodItemDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var sellerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var result = await _foodItemRepo.UpdateFoodItemAsync(id, sellerId!, model);
                if (!result.IsSuccess)
                    return NotFound(new { message = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating food item {id}");
                return StatusCode(500, new { message = "Error updating food item" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFoodItem(int id)
        {
            try
            {
                var sellerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var result = await _foodItemRepo.DeleteFoodItemAsync(id, sellerId!);
                if (!result.IsSuccess)
                    return NotFound(new { message = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting food item {id}");
                return StatusCode(500, new { message = "Error deleting food item" });
            }
        }
    }
}