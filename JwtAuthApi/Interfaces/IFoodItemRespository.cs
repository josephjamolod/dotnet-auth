using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Dtos.Foods;
using JwtAuthApi.Helpers.HelperObjects;

namespace JwtAuthApi.Interfaces
{
    public interface IFoodItemRespository
    {
        Task<object> GetAllFoodItemsAsync(AllFoodsQuery queryObject);
        Task<OperationResult<FoodResponseDto, string>> GetFoodItemByIdAsync(int foodId);
        Task<List<FoodResponseDto>> GetFeaturedItemsAsync();
        Task<object> GetSellerMenuAsync(AllFoodsQuery queryObject, string sellerId);

    }
}