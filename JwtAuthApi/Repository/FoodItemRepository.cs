using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Data;
using JwtAuthApi.Dtos.Foods;
using JwtAuthApi.Interfaces;
using JwtAuthApi.Mappers;
using JwtAuthApi.Models;
using JwtAuthApi.Repository.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthApi.Repository
{
    public class FoodItemRepository : IFoodItemRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDBContext _context;

        public FoodItemRepository(UserManager<AppUser> userManager, ApplicationDBContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<OperationResult<FoodResponseDto, string>> FindById(int foodId, string sellerId)
        {
            var seller = await _userManager.FindByIdAsync(sellerId);
            if (seller == null)
                return OperationResult<FoodResponseDto, string>.Failure("Seller Not Found");

            var foodItem = await _context.FoodItems.FirstOrDefaultAsync(f => f.Id == foodId && f.SellerId == seller.Id);

            if (foodItem == null)
                return OperationResult<FoodResponseDto, string>.Failure("Food item not found"); ;

            var foodItemResponse = foodItem.FoodItemToFoodResponseDto();
            return OperationResult<FoodResponseDto, string>.Success(foodItemResponse);
        }

    }
}