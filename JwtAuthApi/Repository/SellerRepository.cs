using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Data;
using JwtAuthApi.Dtos.Seller;
using JwtAuthApi.Helpers.HelperObjects;
using JwtAuthApi.Interfaces;
using JwtAuthApi.Mappers;
using JwtAuthApi.Models;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthApi.Repository
{
    public class SellerRepository : ISellerRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDBContext _context;
        private readonly ICloudinaryService _cloudinaryService;
        public SellerRepository(UserManager<AppUser> userManager, ApplicationDBContext context, ICloudinaryService cloudinary)
        {
            _userManager = userManager;
            _context = context;
            _cloudinaryService = cloudinary;
        }

        public async Task<OperationResult<SellerProfileDto, string>> GetSellerProfileAsync(string sellerId)
        {
            var seller = await _userManager.FindByIdAsync(sellerId);
            if (seller == null)
                return OperationResult<SellerProfileDto, string>.Failure("Seller Not Found");

            var totalOrder = await _context.Orders.Where(o => o.SellerId == seller.Id).CountAsync();
            var totalItems = await _context.FoodItems.Where(f => f.SellerId == seller.Id).CountAsync();

            var sellerProfile = seller.UserToSellerProfileDto();
            sellerProfile.TotalItems = totalItems;
            sellerProfile.TotalOrders = totalOrder;

            return OperationResult<SellerProfileDto, string>.Success(sellerProfile);
        }

        public async Task<OperationResult<object, ErrorResult>> UpdateSellerAsync(UpdateSellerProfileDto model, string sellerId)
        {
            var seller = await _userManager.FindByIdAsync(sellerId);
            if (seller == null)
                return OperationResult<object, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status404NotFound,
                    ErrDescription = "Seller Not Found"
                });

            model.UpdateSellerProfileDtoToSeller(seller);

            var result = await _userManager.UpdateAsync(seller);
            if (!result.Succeeded)
                return OperationResult<object, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status400BadRequest,
                    ErrDescription = "Update failed"
                });

            return OperationResult<object, ErrorResult>.Success(new
            {
                message = "Profile updated successfully",
                businessName = seller.BusinessName
            });
        }
        public async Task<OperationResult<object, string>> ToggleStatusAsync(string sellerId)
        {
            var seller = await _userManager.FindByIdAsync(sellerId);
            if (seller == null)
                return OperationResult<object, string>.Failure("Seller Not Found");
            seller.IsActive = !seller.IsActive;
            seller.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(seller);
            return OperationResult<object, string>.Success(new
            {
                message = $"Status changed to {(seller.IsActive ? "Active" : "Inactive")}",
                isActive = seller.IsActive
            });
        }

        public async Task<OperationResult<object, ErrorResult>> UploadLogoAsync(IFormFile logo, string sellerId)
        {

            var seller = await _userManager.FindByIdAsync(sellerId);
            if (seller == null)
                return OperationResult<object, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status404NotFound,
                    ErrDescription = "Seller not found"
                });

            // Validate file 
            if (!IsValidImage(logo))
                return OperationResult<object, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status400BadRequest,
                    ErrDescription = "Invalid file type Or File size must not exceed 5MB."
                });

            // Check if seller already has a logo - delete old one first
            var existingLogo = await _context.Logos
             .FirstOrDefaultAsync(l => l.SellerId == sellerId);

            if (existingLogo != null)
            {
                // Delete old logo from Cloudinary
                await _cloudinaryService.DeleteImageAsync(existingLogo.PublicId);

                // Remove old logo from database
                _context.Logos.Remove(existingLogo);
            }

            // Upload to Cloudinary
            var uploadResult = await _cloudinaryService.UploadImageAsync(
                logo,
                $"logos/seller_{sellerId}"
            );

            if (uploadResult == null || string.IsNullOrEmpty(uploadResult.SecureUrl?.ToString()))
                return OperationResult<object, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status500InternalServerError,
                    ErrDescription = "Failed to upload logo to cloud storage."
                });

            // Create new logo entity
            var logoImage = new Logo
            {
                ImageUrl = uploadResult.SecureUrl.ToString(),
                PublicId = uploadResult.PublicId,
                SellerId = sellerId,
                UploadedAt = DateTime.UtcNow
            };

            // Add logo to database
            _context.Logos.Add(logoImage);

            // Update seller's UpdatedAt timestamp
            seller.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(seller);

            // Save all changes
            await _context.SaveChangesAsync();

            // Return success response
            return OperationResult<object, ErrorResult>.Success(new
            {
                message = "Logo uploaded successfully",
                logo = new
                {
                    id = logoImage.Id,
                    url = logoImage.ImageUrl,
                    publicId = logoImage.PublicId,
                    uploadedAt = logoImage.UploadedAt
                }
            });
        }

        private static bool IsValidImage(IFormFile logo)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(logo.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return false;

            // Validate file size (5MB)
            if (logo.Length > 5 * 1024 * 1024)
                return false;
            return true;
        }
    }
}