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
    }
}