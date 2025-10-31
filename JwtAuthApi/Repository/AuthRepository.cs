using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Dtos;
using JwtAuthApi.Interfaces;
using JwtAuthApi.Models;
using JwtAuthApi.Repository.Models;
using Microsoft.AspNetCore.Identity;

namespace JwtAuthApi.Repository
{
    public class AuthRepository : IAuthRepository

    {
        private readonly UserManager<AppUser> _userManager;
        public AuthRepository(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<OperationResult<AppUser, string>> CreateUserAsync(RegisterDto model)
        {
            if (await UsernameExistsAsync(model.Username) || await EmailExistsAsync(model.Email))
                return OperationResult<AppUser, string>.Failure("User already exists");

            var user = new AppUser
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return OperationResult<AppUser, string>.Failure("User creation failed");

            await _userManager.AddToRoleAsync(user, "User");
            return OperationResult<AppUser, string>.Success(user);
        }

        private async Task<bool> UsernameExistsAsync(string username)
           => await _userManager.FindByNameAsync(username) != null;
        private async Task<bool> EmailExistsAsync(string email)
            => await _userManager.FindByEmailAsync(email) != null;
    }
}