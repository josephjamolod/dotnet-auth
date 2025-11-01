using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JwtAuthApi.Data;
using JwtAuthApi.Dtos;
using JwtAuthApi.Interfaces;
using JwtAuthApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<AuthController> _logger;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        private readonly ApplicationDBContext _context;
        private readonly IAuthRepository _authRepo;
        public AuthController(
            UserManager<AppUser> userManager,
            ILogger<AuthController> logger,
            IEmailService emailService,
            ITokenService tokenService,
            ApplicationDBContext context,
            IAuthRepository authRepo
            )
        {
            _userManager = userManager;
            _logger = logger;
            _emailService = emailService;
            _tokenService = tokenService;
            _context = context;
            _authRepo = authRepo;

        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authRepo.CreateUserAsync(model);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            //generate email confirmation token
            await SendEmailConfirmationAsync(result.Value!);

            _logger.LogInformation($"User '{model.Username}' registered successfully");
            return Ok(new
            {
                message = "Registration successful! Please check your email to confirm your account."
            });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmEmailDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authRepo.ConfirmEmailAsync(model);

            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(new { message = result.Error });
        }

        // Resend email confirmation link
        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authRepo.ResendEmailConfirmationAsync(model);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            var user = result.Value;

            if (user == null)
                return Ok(new { messsage = "If the email exists, a confirmation link has been sent" });

            // Generate new confirmation token
            await SendEmailConfirmationAsync(user);

            // Update rate limiting timestamp
            user.EmailConfirmationLastSent = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                message = "Confirmation email has been resent",
                canResendAgainIn = "2 minutes"
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _authRepo.LoginAsync(model);
                if (!result.IsSuccess)
                    return Unauthorized(new { message = result.Error });

                var user = result.Value!;
                // Check if 2FA is enabled for this user
                if (user.TwoFactorEnabled)
                {
                    await SendNew2FACodeAsync(user);
                    _logger.LogInformation($"2FA code sent to {model.UserName}");

                    return Ok(new AuthResponseDto
                    {
                        RequiresTwoFactor = true,
                        Username = user.UserName!,
                        Email = user.Email!
                    });
                }

                var authResponse = await _authRepo.SaveRefreshToken(user, GetIpAddress());

                _logger.LogInformation($"User '{model.UserName}' logged in successfully from {GetIpAddress()}");

                return Ok(authResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error during login for user: {model.UserName}");
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again." });
            }
        }


        //Verify 2fa and complete the login process
        [HttpPost("verify-2fa")]
        public async Task<IActionResult> Verify2FA([FromBody] Verify2FADto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _authRepo.Verify2FAAsync(model.Username, model.Code);
                if (!result.IsSuccess)
                    return Unauthorized(new { message = result.Error });

                var user = result.Value!;

                // Generate tokens
                var authResponse = await _authRepo.SaveRefreshToken(user, GetIpAddress());

                _logger.LogInformation($"2FA verification successful for user '{model.Username}'");

                return Ok(authResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error during login for user: {model.Username}");
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again." });
            }
        }

        // RESEND 2FA CODE ENDPOINT 
        [HttpPost("resend-2fa-code")]
        public async Task<IActionResult> Resend2FACode([FromBody] Resend2FACodeDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _authRepo.Resend2FACodeAsync(model.Username);
                if (!result.IsSuccess)
                    return Unauthorized(new { message = result.Error });

                var user = result.Value;

                if (user == null)
                    return Ok(new { message = "If your account has 2FA enabled, a new code has been sent" });

                await SendNew2FACodeAsync(user);
                _logger.LogInformation($"2FA code resent to {model.Username}");

                return Ok(new
                {
                    message = "A new verification code has been sent to your email",
                    expiresIn = "5 minutes"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again." });
            }
        }

        [HttpPost("enable-2fa")]
        [Authorize]
        public async Task<IActionResult> Enable2FA()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Invalid authentication token" });

                var result = await _authRepo.Enable2FAAsync(userId);
                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });
                var user = result.Value;

                if (user == null)
                    return NotFound(new { message = "User not found" });

                _logger.LogInformation($"2FA enabled for user '{user.UserName}'");

                return Ok(new
                {
                    message = "Two-factor authentication has been enabled successfully"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again." });
            }
        }

        [HttpPost("disable-2fa")]
        [Authorize]
        public async Task<IActionResult> Disable2FA()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Invalid authentication token" });

                var result = await _authRepo.Disable2FAAsync(userId);
                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });
                var user = result.Value;

                if (user == null)
                    return NotFound(new { message = "User not found" });

                _logger.LogInformation($"2FA disabled for user '{user.UserName}'");
                return Ok(new
                {
                    message = "Two-factor authentication has been disabled successfully"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again." });
            }
        }

        /// Refresh expired access token using refresh token
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                // Find refresh token in database
                var token = await _context.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == model.RefreshToken);

                if (token == null || !token.IsActive)
                {
                    _logger.LogWarning("Invalid refresh token attempt");
                    return Unauthorized(new { message = "Invalid or expired refresh token" });
                }

                var user = token.User;

                // Generate new tokens
                var authResponse = await _authRepo.SaveRefreshToken(user, GetIpAddress(), token);

                _logger.LogInformation($"Token refreshed for user '{user.UserName}'");

                return Ok(authResponse);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again." });
            }

        }

        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var token = await _authRepo.RevokeTokenAsync(model.RefreshToken, GetIpAddress());
                if (token == null || !token.IsActive)
                    //     return NotFound(new { message = "Token not found or already revoked" });
                    _logger.LogInformation("Refresh token revoked successfully");

                return Ok(new { message = "Token revoked successfully" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again." });
            }
        }

        /// Request password reset
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.EmailConfirmed)
                return Ok(new
                {
                    message = "If the email exists, a password reset link has been sent"
                });
            // Generate password reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _emailService.SendPasswordResetEmailAsync(user.Email!, resetToken);
            _logger.LogInformation($"Password reset requested for email: {model.Email}");

            return Ok(new
            {
                message = "  password reset link has been sent"
            });
        }

        /// Reset password with token
        /// 
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _authRepo.ResetPasswordAsync(model);
                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new
                {
                    message = result.Value
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again." });
            }
        }

        // HELPER METHODS

        private async Task SendNew2FACodeAsync(AppUser user)
        {
            var code = GenerateRandom6DigitCode();
            user.TwoFactorCode = code;
            user.TwoFactorCodeExpiry = DateTime.UtcNow.AddMinutes(5);
            user.TwoFactorCodeLastSent = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await _emailService.Send2FACodeAsync(user.Email!, code);
        }
        private async Task SendEmailConfirmationAsync(AppUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action(
                nameof(ConfirmEmail),
                "Auth",
                new { userId = user.Id, token },
                Request.Scheme
                );

            await _emailService.SendEmailConfirmationAsync(user.Email!, confirmationLink!);
        }
        private static string GenerateRandom6DigitCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private string GetIpAddress()
        {
            // Check for forwarded IP (if behind proxy/load balancer)
            if (Request.Headers.TryGetValue("X-Forwarded-For", out Microsoft.Extensions.Primitives.StringValues value))
                return value!;

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

    }
}