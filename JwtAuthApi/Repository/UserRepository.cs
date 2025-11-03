using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Data;
using JwtAuthApi.Dtos.User;
using JwtAuthApi.Interfaces;
using JwtAuthApi.Mappers;
using JwtAuthApi.Models;
using JwtAuthApi.Repository.Models;
using Microsoft.AspNetCore.Identity;

namespace JwtAuthApi.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDBContext _context;

        public UserRepository(UserManager<AppUser> userManager, ApplicationDBContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<OperationResult<UserProfileDto, ErrorResult>> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null)
                return OperationResult<UserProfileDto, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status404NotFound,
                    ErrDescription = "User Not Found"
                });
            var roles = await _userManager.GetRolesAsync(user);
            var profile = user.ToUserProfileDto();
            profile.Roles = [.. roles];

            return OperationResult<UserProfileDto, ErrorResult>.Success(profile);
        }

        public async Task<OperationResult<object, ErrorResult>> UpdateProfileAsync(string userId, UpdateProfileDto model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return OperationResult<object, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status404NotFound,
                    ErrDescription = "User Not Found"
                });
            if (user.UserName != model.UserName)
            {
                var existingUser = await _userManager.FindByNameAsync(model.UserName);
                if (existingUser != null)
                {
                    return OperationResult<object, ErrorResult>.Failure(new ErrorResult()
                    {
                        ErrCode = StatusCodes.Status400BadRequest,
                        ErrDescription = "Username is already taken"
                    });
                }
                user.UserName = model.UserName;
                user.NormalizedUserName = model.UserName.ToUpper();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return OperationResult<object, ErrorResult>.Failure(new ErrorResult()
                {
                    ErrCode = StatusCodes.Status400BadRequest,
                    ErrDescription = "Failed to update profile"
                });

            return OperationResult<object, ErrorResult>.Success(new
            {
                message = "Profile updated successfully",
                username = user.UserName,
                firstName = user.FirstName,
                lastName = user.LastName
            });

        }
    }
}