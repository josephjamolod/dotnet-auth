using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Dtos;
using JwtAuthApi.Dtos.Auth;
using JwtAuthApi.Dtos.Seller;
using JwtAuthApi.Dtos.User;
using JwtAuthApi.Models;

namespace JwtAuthApi.Mappers
{
    public static class UserMappers
    {
        public static UserProfileDto ToUserProfileDto(this AppUser appUser)
        {
            return new UserProfileDto()
            {
                UserId = appUser.Id,
                Username = appUser.UserName!,
                Email = appUser.Email!,
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                EmailConfirmed = appUser.EmailConfirmed,
                TwoFactorEnabled = appUser.TwoFactorEnabled,
                CreatedAt = appUser.LockoutEnd?.DateTime ?? DateTime.UtcNow // Use appropriate date field
            };
        }
        public static AppUser ToUserFromRegisterDto(this RegisterDto model)
        {
            return new AppUser()
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };
        }
        public static AppUser ToUserFromRegisterSellerDto(this RegisterSellerDto model)
        {
            return new AppUser()
            {

                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                BusinessName = model.BusinessName,
                BusinessNumber = model.BusinessNumber,
                Address = model.Address,
                City = model.City,
                State = model.State,
                PostalCode = model.PostalCode,
                Description = model.Description,
                IsApproved = false, // Requires approval
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}