using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Dtos.Foods;
using JwtAuthApi.Models;

namespace JwtAuthApi.Mappers
{
    public static class FoodMapper
    {
        public static FoodResponseDto FoodItemToFoodResponseDto(this FoodItem f)
        {
            return new FoodResponseDto()
            {
                Id = f.Id,
                Name = f.Name,
                Description = f.Description,
                Price = f.Price,
                Category = f.Category,
                ImageUrls = f.ImageUrls
                    .Select(img => new FoodImageResponseDto
                    {
                        Id = img.Id,
                        Url = img.ImageUrl,
                        PublicId = img.PublicId,
                        IsMain = img.IsMainImage,
                        UploadedAt = img.UploadedAt
                    })
                    .OrderByDescending(img => img.IsMain)
                    .ToList(),
                IsAvailable = f.IsAvailable,
                PreparationTime = f.PreparationTime,
                Rating = f.Rating,
                TotalRatings = f.TotalRatings,
                TotalSold = f.TotalSold,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            }
            ;

        }
    }
}