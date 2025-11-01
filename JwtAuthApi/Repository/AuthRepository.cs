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
        public async Task<OperationResult<object, string>> ConfirmEmailAsync(ConfirmEmailDto model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return OperationResult<object, string>.Failure("User Not found");
            var result = await _userManager.ConfirmEmailAsync(user, model.Token);

            if (result.Succeeded)
                return OperationResult<object, string>.Success(new
                {
                    message = "Email confirmed successfully! You can now log in to your account."
                });

            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return OperationResult<object, string>.Failure(errors);
        }
        public async Task<OperationResult<AppUser, string>> ResendEmailConfirmationAsync(ResendConfirmationDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return OperationResult<AppUser, string>.Failure("If the email exists, a confirmation link has been sent");

            if (user.EmailConfirmed)
                return OperationResult<AppUser, string>.Failure("Email is already confirmed");

            // ✅ RATE LIMITING: Allow resend only after 2 minutes
            if (user.EmailConfirmationLastSent.HasValue)
            {
                var timeSinceLastSent = DateTime.UtcNow - user.EmailConfirmationLastSent.Value;
                var waitTimeMinutes = 2;

                if (timeSinceLastSent.TotalMinutes < waitTimeMinutes)
                {
                    var secondsRemaining = (int)((waitTimeMinutes * 60) - timeSinceLastSent.TotalSeconds);

                    return OperationResult<AppUser, string>.Failure(
                   $"Please wait {secondsRemaining} seconds before requesting another confirmation email"
                     );
                }
            }
            return OperationResult<AppUser, string>.Success(user);
        }

        public async Task<OperationResult<AppUser, string>> LoginAsync(LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
                return OperationResult<AppUser, string>.Failure("User Cannot be found");

            var isCorrectPassword = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isCorrectPassword)
                return OperationResult<AppUser, string>.Failure("Invalid Credentials!");

            if (!user.EmailConfirmed)
                return OperationResult<AppUser, string>.Failure("Please confirm your email before logging in. Check your inbox for confirmation link.");

            return OperationResult<AppUser, string>.Success(user);
        }

        private async Task<bool> UsernameExistsAsync(string username)
           => await _userManager.FindByNameAsync(username) != null;
        private async Task<bool> EmailExistsAsync(string email)
            => await _userManager.FindByEmailAsync(email) != null;

    }
}