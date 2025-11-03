using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    }
}