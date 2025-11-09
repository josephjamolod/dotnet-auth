using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Helpers.HelperObjects;
using JwtAuthApi.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoodItemController : ControllerBase
    {
        private readonly IFoodItemRespository _foodItemRepo;
        public FoodItemController(IFoodItemRespository foodItemRepo)
        {
            _foodItemRepo = foodItemRepo;

        }
        [HttpGet]
        public async Task<ActionResult> GetAllFoodItems([FromQuery] AllFoodsQuery queryObject)
        {
            try
            {
                var result = await _foodItemRepo.GetAllFoodItemsAsync(queryObject);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error retrieving food items" });
            }
        }
    }
}