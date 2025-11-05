using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Data;
using JwtAuthApi.Dtos;
using JwtAuthApi.Dtos.Seller;
using JwtAuthApi.Interfaces;
using JwtAuthApi.Mappers;
using JwtAuthApi.Models;
using JwtAuthApi.Repository.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthApi.Repository
{
    public class AuthRepository : IAuthRepository

    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDBContext _context;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        public AuthRepository(UserManager<AppUser> userManager, ApplicationDBContext context, ITokenService tokenService, IEmailService emailService)
        {
            _userManager = userManager;
            _context = context;
            _tokenService = tokenService;
            _emailService = emailService;

        }
        public async Task<OperationResult<object, string>> RegisterSellerAsync(RegisterSellerDto model, Func<AppUser, string, Task<string?>> genConfirmationLink)
        {
            if (await EmailExistsAsync(model.Email))
                return OperationResult<object, string>.Failure("Email already exists");

            var existingBusiness = await _userManager.Users
                .AnyAsync(u => u.BusinessNumber == model.BusinessNumber);
            if (existingBusiness)
                return OperationResult<object, string>.Failure("Business number is already registered");

            var seller = model.ToUserFromRegisterSellerDto();
            var result = await _userManager.CreateAsync(seller, model.Password);

            if (!result.Succeeded)
                return OperationResult<object, string>.Failure("Registration failed");

            // Assign Seller role
            await _userManager.AddToRoleAsync(seller, "Seller");
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(seller);

            // Send confirmation email
            var confirmationLink = await genConfirmationLink(seller, token);
            await SendEmailConfirmationAsync(seller, confirmationLink!);

            return OperationResult<object, string>.Success(new
            {
                message = "Registration successful! Please check your email to confirm your account. You will be notified via email once admin approved.",
                businessName = model.BusinessName,
                status = "Pending Approval"
            });
        }

        public async Task<OperationResult<object, string>> CreateUserAsync(RegisterDto model, Func<AppUser, string, Task<string?>> genConfirmationLink)
        {
            if (await UsernameExistsAsync(model.Username) || await EmailExistsAsync(model.Email))
                return OperationResult<object, string>.Failure("User already exists");

            var user = model.ToUserFromRegisterDto();

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return OperationResult<object, string>.Failure("User creation failed");

            await _userManager.AddToRoleAsync(user, "User");
            //generate email confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = await genConfirmationLink(user, token);
            await SendEmailConfirmationAsync(user, confirmationLink!);

            return OperationResult<object, string>.Success(new
            {
                message = "Registration successful! Please check your email to confirm your account.",
                status = "Approved"
            });
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
        public async Task<OperationResult<AppUser?, string>> ResendEmailConfirmationAsync(ResendConfirmationDto model, Func<AppUser, string, Task<string?>> genConfirmationLink)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return OperationResult<AppUser?, string>.Success(null);

            if (user.EmailConfirmed)
                return OperationResult<AppUser?, string>.Failure("Email is already confirmed");

            // ✅ RATE LIMITING: Allow resend only after 2 minutes
            if (user.EmailConfirmationLastSent.HasValue)
            {
                var timeSinceLastSent = DateTime.UtcNow - user.EmailConfirmationLastSent.Value;
                var waitTimeMinutes = 2;

                if (timeSinceLastSent.TotalMinutes < waitTimeMinutes)
                {
                    var secondsRemaining = (int)((waitTimeMinutes * 60) - timeSinceLastSent.TotalSeconds);

                    return OperationResult<AppUser?, string>.Failure(
                   $"Please wait {secondsRemaining} seconds before requesting another confirmation email"
                     );
                }
            }
            // Generate new confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = await genConfirmationLink(user, token);
            await SendEmailConfirmationAsync(user, confirmationLink!);

            return OperationResult<AppUser?, string>.Success(user);
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
            if (user.TwoFactorEnabled)
            {
                await SendNew2FACodeAsync(user);
                return OperationResult<AppUser, string>.Success(user);
            }
            return OperationResult<AppUser, string>.Success(user);
        }

        public async Task<OperationResult<AppUser, string>> Verify2FAAsync(string username, string code)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return OperationResult<AppUser, string>.Failure("Invalid verification request");
            }
            // Validate 2FA code
            if (user.TwoFactorCode != code ||
                user.TwoFactorCodeExpiry == null ||
                user.TwoFactorCodeExpiry < DateTime.UtcNow)
                return OperationResult<AppUser, string>.Failure("Invalid or expired verification code. Please request a new code.");

            // Clear 2FA code after successful verification
            user.TwoFactorCode = null;
            user.TwoFactorCodeExpiry = null;
            await _userManager.UpdateAsync(user);
            return OperationResult<AppUser, string>.Success(user);
        }

        public async Task<OperationResult<AppUser?, string>> Resend2FACodeAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null || !user.TwoFactorEnabled)
                return OperationResult<AppUser?, string>.Success(null);

            // Rate limiting: Wait 1 minute between requests
            if (user.TwoFactorCodeLastSent.HasValue)
            {
                var timeSinceLastSent = DateTime.UtcNow - user.TwoFactorCodeLastSent.Value;
                var waitTimeMinutes = 1; // Wait 1 minute between requests

                if (timeSinceLastSent.TotalMinutes < waitTimeMinutes)
                {
                    var secondsRemaining = (int)((waitTimeMinutes * 60) - timeSinceLastSent.TotalSeconds);
                    return OperationResult<AppUser?, string>.Failure($"Please wait {secondsRemaining} seconds before requesting a new code");
                }
            }
            await SendNew2FACodeAsync(user);
            return OperationResult<AppUser?, string>.Success(user);
        }
        public async Task<OperationResult<AppUser?, string>> Enable2FAAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return OperationResult<AppUser?, string>.Success(null);

            if (user.TwoFactorEnabled)
                return OperationResult<AppUser?, string>.Failure("2FA already enabled");

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            return OperationResult<AppUser?, string>.Success(user);
        }
        public async Task<OperationResult<AppUser?, string>> Disable2FAAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return OperationResult<AppUser?, string>.Success(null);

            if (!user.TwoFactorEnabled)
                return OperationResult<AppUser?, string>.Failure("2FA already disabled");

            await _userManager.SetTwoFactorEnabledAsync(user, false);

            // Clear any existing 2FA codes
            user.TwoFactorCode = null;
            user.TwoFactorCodeExpiry = null;
            await _userManager.UpdateAsync(user);
            return OperationResult<AppUser?, string>.Success(user);
        }

        public async Task<RefreshToken?> RefreshTokenAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens
                         .Include(rt => rt.User)
                         .FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (token == null)
                return null;
            return token;
        }

        public async Task<AuthResponseDto> SaveRefreshToken(AppUser user, string ipAddress, RefreshToken? token = default)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken(ipAddress);

            if (token != null)
                await TokenRevokerAsync([token], ipAddress);

            // Save refresh token to database
            refreshToken.UserId = user.Id;
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                AccessTokenExpiration = DateTime.UtcNow.AddMinutes(15),
                Username = user.UserName!,
                Email = user.Email!,
                Roles = [.. roles],
                RequiresTwoFactor = false
            };
        }

        public async Task<OperationResult<object, string>> RevokeTokenAsync(string refreshToken, string ipAddress)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (token == null || !token.IsActive)
                return OperationResult<object, string>.Failure("Token not found or already revoked");
            await TokenRevokerAsync([token], ipAddress);
            return OperationResult<object, string>.Success(new { message = "Token revoked successfully" });
        }

        public async Task<string?> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !user.EmailConfirmed)
                return null;
            // Generate password reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _emailService.SendPasswordResetEmailAsync(email, resetToken);
            return resetToken;
        }

        public async Task<OperationResult<string, string>> ResetPasswordAsync(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return OperationResult<string, string>.Failure("Invalid request");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, "newpassword");
            if (result.Succeeded)
            {
                // Revoke all active refresh tokens for security
                var tokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == user.Id && rt.IsActive)
                    .ToListAsync();

                await TokenRevokerAsync(tokens, "PasswordReset");

                return OperationResult<string, string>.Success("Password has been reset successfully. Please log in with your new password.");
            }
            return OperationResult<string, string>.Failure("Password reset failed. The link may be expired or invalid.");
        }

        //HELPERS
        private async Task SendEmailConfirmationAsync(AppUser user, string confirmationLink)
        {
            await _emailService.SendEmailConfirmationAsync(user.Email!, confirmationLink!);
            // Update rate limiting timestamp
            user.EmailConfirmationLastSent = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }

        private async Task SendNew2FACodeAsync(AppUser user)
        {
            var code = GenerateRandom6DigitCode();
            user.TwoFactorCode = code;
            user.TwoFactorCodeExpiry = DateTime.UtcNow.AddMinutes(5);
            user.TwoFactorCodeLastSent = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await _emailService.Send2FACodeAsync(user.Email!, code);
        }

        private static string GenerateRandom6DigitCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private async Task<bool> UsernameExistsAsync(string username)
           => await _userManager.FindByNameAsync(username) != null;
        private async Task<bool> EmailExistsAsync(string email)
            => await _userManager.FindByEmailAsync(email) != null;

        private async Task TokenRevokerAsync(List<RefreshToken> tokens, string reason)
        {
            if (tokens == null || tokens.Count == 0)
                return;

            foreach (var token in tokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedByIp = reason;  // Clear reason for auditing
            }

            await _context.SaveChangesAsync();
        }
    }
}