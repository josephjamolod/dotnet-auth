using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Data;
using JwtAuthApi.Dtos.Foods;
using JwtAuthApi.Interfaces;
using JwtAuthApi.Mappers;
using JwtAuthApi.Models;
using JwtAuthApi.Repository.HelperObjects;
using JwtAuthApi.Repository.Models;
using JwtAuthApi.Repository.QueryBuilders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthApi.Repository
{
    public class FoodItemRepository : IFoodItemRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDBContext _context;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<FoodItemRepository> _logger;

        public FoodItemRepository(UserManager<AppUser> userManager, ApplicationDBContext context, ICloudinaryService cloudinaryService, ILogger<FoodItemRepository> logger)
        {
            _userManager = userManager;
            _context = context;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        public async Task<object> GetAllFoodItemsAsync(AllFoodsQuery queryObject, string sellerId)
        {
            var query = _context.FoodItems
                  .Include(f => f.ImageUrls)
                  .Where(f => f.SellerId == sellerId)
                  .AsQueryable();

            // Apply filters
            query = FoodItemQueryBuilder.ApplyFilters(query, queryObject);
            // Apply sorting
            query = FoodItemQueryBuilder.ApplySorting(query, queryObject);

            // Get total count before pagination
            var totalCount = await query.CountAsync();
            // Apply pagination
            var skip = (queryObject.PageNumber - 1) * queryObject.PageSize;

            //  Materialize the data from database
            var foodItemsFromDb = await query
                .Skip(skip)
                .Take(queryObject.PageSize)
                .ToListAsync();

            //  Apply the mapper in-memory
            var foodItems = foodItemsFromDb
                .Select(f => f.FoodItemToFoodResponseDto())
                .ToList();

            return new
            {
                total = totalCount,
                pageNumber = queryObject.PageNumber,
                pageSize = queryObject.PageSize,
                items = foodItems
            };
        }

        public async Task<OperationResult<FoodResponseDto, string>> GetByIdAsync(int foodId, string sellerId)
        {
            var seller = await _userManager.FindByIdAsync(sellerId);
            if (seller == null)
                return OperationResult<FoodResponseDto, string>.Failure("Seller Not Found");

            var foodItem = await _context.FoodItems.Include(f => f.ImageUrls).FirstOrDefaultAsync(f => f.Id == foodId && f.SellerId == seller.Id);

            if (foodItem == null)
                return OperationResult<FoodResponseDto, string>.Failure("Food item not found"); ;

            var foodItemResponse = foodItem.FoodItemToFoodResponseDto();
            return OperationResult<FoodResponseDto, string>.Success(foodItemResponse);
        }

        public async Task<OperationResult<FoodItem, string>> CreateAsync(CreateFoodItemDto model, string sellerId)
        {
            var seller = await _userManager.FindByIdAsync(sellerId);
            if (seller == null)
                return OperationResult<FoodItem, string>.Failure("Seller Not Found");
            var foodItem = model.CreateFoodItemDtoToFoodItem();
            foodItem.SellerId = seller.Id;

            _context.FoodItems.Add(foodItem);
            await _context.SaveChangesAsync();

            return OperationResult<FoodItem, string>.Success(foodItem);
        }

        public async Task<OperationResult<object, ErrorResult>> UploadFoodImagesAsync(int foodId, List<IFormFile> images, bool setFirstAsMain, string sellerId)
        {
            var foodItem = await _context.FoodItems
                    .Include(f => f.ImageUrls)
                    .FirstOrDefaultAsync(f => f.Id == foodId && f.SellerId == sellerId);

            if (foodItem == null)
                return OperationResult<object, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status404NotFound,
                    ErrDescription = "Food item not found"
                });

            // Check total images after upload
            if (foodItem.ImageUrls.Count + images.Count > 5)
                return OperationResult<object, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status404NotFound,
                    ErrDescription = $"Cannot upload {images.Count} images. Maximum 5 images total. Current: {foodItem.ImageUrls.Count}"
                });

            var uploadedImages = new List<object>();
            var isFirstImage = !foodItem.ImageUrls.Any();

            foreach (var (image, index) in images.Select((img, idx) => (img, idx)))
            {
                // Validate file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    _logger.LogWarning($"Invalid file type: {extension}");
                    continue;
                }

                if (image.Length > 5 * 1024 * 1024) // 5MB
                {
                    _logger.LogWarning($"File too large: {image.Length} bytes");
                    continue;
                }

                // Upload to Cloudinary
                var uploadResult = await _cloudinaryService.UploadImageAsync(
                    image,
                    $"food-items/{sellerId}/{foodId}"
                );

                if (uploadResult == null || string.IsNullOrEmpty(uploadResult.SecureUrl?.ToString()))
                {
                    _logger.LogError("Cloudinary upload failed");
                    continue;
                }

                // Save to database
                var foodImage = new FoodImage
                {
                    ImageUrl = uploadResult.SecureUrl.ToString(),
                    PublicId = uploadResult.PublicId,
                    FoodItemId = foodId,
                    //When uploading images for a food item, only one image becomes the “main” one.
                    //The first uploaded image gets priority,but only if allowed(setFirstAsMain = true).
                    //If the food item already has a main image, it won’t be overridden unless no main image exists.
                    IsMainImage = (isFirstImage && index == 0 && setFirstAsMain) ||
                                 (!isFirstImage && index == 0 && setFirstAsMain && !foodItem.ImageUrls.Any(img => img.IsMainImage)),
                    UploadedAt = DateTime.UtcNow
                };

                _context.FoodImages.Add(foodImage);
                await _context.SaveChangesAsync();

                uploadedImages.Add(new
                {
                    id = foodImage.Id,
                    url = foodImage.ImageUrl,
                    isMain = foodImage.IsMainImage
                });

                _logger.LogInformation($"Image uploaded for food item {foodId}: {uploadResult.PublicId}");
            }

            foodItem.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return OperationResult<object, ErrorResult>.Success(new
            {
                message = $"{uploadedImages.Count} image(s) uploaded successfully",
                images = uploadedImages,
                totalImages = foodItem.ImageUrls.Count
            });
        }

        public async Task<OperationResult<object, string>> DeleteFoodImageAsync(int imageId, string sellerId)
        {
            var image = await _context.FoodImages
                        .Include(fi => fi.FoodItem)
                        .FirstOrDefaultAsync(fi => fi.Id == imageId && fi.FoodItem.SellerId == sellerId);

            if (image == null)
                return OperationResult<object, string>.Failure("Image not found");

            // Delete from Cloudinary
            var deleteResult = await _cloudinaryService.DeleteImageAsync(image.PublicId);

            if (deleteResult.Result != "ok")
                _logger.LogWarning($"Failed to delete image from Cloudinary: {image.PublicId}");


            // Delete from database
            _context.FoodImages.Remove(image);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Image deleted: {image.PublicId}");

            return OperationResult<object, string>.Success(new { message = "Image deleted successfully" });
        }

        public async Task<OperationResult<object, string>> SetMainFoodImageAsync(int imageId, string sellerId)
        {
            var image = await _context.FoodImages
                          .Include(fi => fi.FoodItem)
                              .ThenInclude(f => f.ImageUrls)
                          .FirstOrDefaultAsync(fi => fi.Id == imageId && fi.FoodItem.SellerId == sellerId);

            if (image == null)
                return OperationResult<object, string>.Failure("Image not found");

            // Remove main flag from all other images
            foreach (var img in image.FoodItem.ImageUrls)
            {
                img.IsMainImage = false;
            }

            // Set this image as main
            image.IsMainImage = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Main image set for food item {image.FoodItemId}: {imageId}");

            return OperationResult<object, string>.Success(new
            {
                message = "Main image updated successfully",
                imageUrl = image.ImageUrl
            });
        }

        public async Task<OperationResult<object, string>> UpdateFoodItemAsync(int foodId, string sellerId, UpdateFoodItemDto model)
        {
            var foodItem = await _context.FoodItems
                        .FirstOrDefaultAsync(f => f.Id == foodId && f.SellerId == sellerId);

            if (foodItem == null)
                return OperationResult<object, string>.Failure("Food item not found");

            model.UpdateFoodItemDtoToFoodItem(foodItem);

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Food item updated: {foodItem.Name}");

            return OperationResult<object, string>.Success(new
            {
                message = "Food item updated successfully",
                id = foodItem.Id,
                name = foodItem.Name
            });

        }
    }
}