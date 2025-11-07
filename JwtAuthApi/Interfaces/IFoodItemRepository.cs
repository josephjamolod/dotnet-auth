using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Dtos.Foods;
using JwtAuthApi.Models;
using JwtAuthApi.Repository.Models;

namespace JwtAuthApi.Interfaces
{
    public interface IFoodItemRepository
    {
        Task<OperationResult<FoodResponseDto, string>> GetByIdAsync(int foodId, string sellerId);
        Task<OperationResult<FoodItem, string>> CreateAsync(CreateFoodItemDto model, string sellerId);
        Task<OperationResult<object, ErrorResult>> UploadFoodImagesAsync(int foodId, List<IFormFile> images, bool setFirstAsMain, string sellerId);
        Task<OperationResult<object, string>> DeleteFoodImageAsync(int imageId, string sellerId);
        Task<OperationResult<object, string>> SetMainFoodImageAsync(int imageId, string sellerId);
        Task<OperationResult<object, string>> UpdateFoodItemAsync(int foodId, string sellerId, UpdateFoodItemDto model);
    }
}