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
        public AuthController(UserManager<AppUser> userManager, ILogger<AuthController> logger, IEmailService emailService, ITokenService tokenService, ApplicationDBContext context)
        {
            _userManager = userManager;
            _logger = logger;
            _emailService = emailService;
            _tokenService = tokenService;
            _context = context;

        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //check if username already exist
            var existingUser = await _userManager.FindByNameAsync(model.Username);
            if (existingUser != null)
            {
                return BadRequest(new { message = "User already exist!" });
            }

            //check if email already exist
            existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email already exist!" });
            }

            //create AppUser instance
            var user = new AppUser()
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);

            }
            await _userManager.AddToRoleAsync(user, "User");

            //generate email confirmation token
            await SendEmailConfirmationAsync(user);

            _logger.LogInformation($"User '{model.Username}' registered successfully");

            return Ok(new
            {
                message = "Registration successful! Please check your email to confirm your account."
            });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Invalid confirmation link" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return BadRequest(new { message = "User not found" });

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                _logger.LogInformation($"Email confirmed for user '{user.UserName}'");
                return Ok(new
                {
                    message = "Email confirmed successfully! You can now log in to your account."
                });
            }

            return BadRequest(new
            {
                message = "Email confirmation failed. The link may be expired or invalid.",
                errors = result.Errors
            });
        }

        // Resend email confirmation link
        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation([FromBody] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            // Don't reveal if email exists (security)
            if (user == null)
                return Ok(new { message = "If the email exists, a confirmation link has been sent" });

            if (user.EmailConfirmed)
                return BadRequest(new { message = "Email is already confirmed" });

            // ✅ RATE LIMITING: Allow resend only after 2 minutes
            if (user.EmailConfirmationLastSent.HasValue)
            {
                var timeSinceLastSent = DateTime.UtcNow - user.EmailConfirmationLastSent.Value;
                var waitTimeMinutes = 2;

                if (timeSinceLastSent.TotalMinutes < waitTimeMinutes)
                {
                    var secondsRemaining = (int)((waitTimeMinutes * 60) - timeSinceLastSent.TotalSeconds);

                    return BadRequest(new
                    {
                        message = $"Please wait {secondsRemaining} seconds before requesting another confirmation email",
                        canResendIn = secondsRemaining
                    });
                }
            }
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
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                _logger.LogWarning($"Failed login attempt for username: {model.UserName}");
                return Unauthorized(new { message = "User Cannot be found" });
            }
            var isCorrectPassword = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isCorrectPassword)
            {
                _logger.LogWarning($"Failed login attempt for username: {model.UserName} - Invalid password");
                return Unauthorized(new { message = "Invalid Credentials!" });
            }
            if (!user.EmailConfirmed)
            {
                return Unauthorized(new
                {
                    message = "Please confirm your email before logging in. Check your inbox for confirmation link."
                });
            }
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

            // Generate tokens
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken(GetIpAddress());

            // Save refresh token to database
            refreshToken.UserId = user.Id;
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"User '{model.UserName}' logged in successfully from {GetIpAddress()}");
            return Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                AccessTokenExpiration = DateTime.UtcNow.AddMinutes(15),
                Username = user.UserName!,
                Email = user.Email!,
                Roles = roles.ToList(),
                RequiresTwoFactor = false
            });
        }

        //Verify 2fa and complete the login process
        [HttpPost("verify-2fa")]
        public async Task<IActionResult> Verify2FA([FromBody] Verify2FADto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                _logger.LogWarning($"2FA verification failed - user not found: {model.Username}");
                return Unauthorized(new { message = "Invalid verification request" });
            }
            // Validate 2FA code
            if (user.TwoFactorCode != model.Code ||
                user.TwoFactorCodeExpiry == null ||
                user.TwoFactorCodeExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning($"Invalid or expired 2FA code for user '{model.Username}'");
                return Unauthorized(new
                {
                    message = "Invalid or expired verification code. Please request a new code."
                });
            }
            // Clear 2FA code after successful verification
            user.TwoFactorCode = null;
            user.TwoFactorCodeExpiry = null;
            await _userManager.UpdateAsync(user);
            // Generate tokens
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken(GetIpAddress());

            refreshToken.UserId = user.Id;
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"2FA verification successful for user '{model.Username}'");

            return Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                AccessTokenExpiration = DateTime.UtcNow.AddMinutes(15),
                Username = user.UserName!,
                Email = user.Email!,
                Roles = roles.ToList(),
                RequiresTwoFactor = false
            });
        }

        // RESEND 2FA CODE ENDPOINT 
        [HttpPost("resend-2fa-code")]
        public async Task<IActionResult> Resend2FACode([FromBody] string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest(new { message = "Username is required" });
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user == null || !user.TwoFactorEnabled)
                return Ok(new { message = "If your account has 2FA enabled, a new code has been sent" });

            // Rate limiting: Wait 1 minute between requests
            if (user.TwoFactorCodeLastSent.HasValue)
            {
                var timeSinceLastSent = DateTime.UtcNow - user.TwoFactorCodeLastSent.Value;
                var waitTimeMinutes = 1; // Wait 1 minute between requests

                if (timeSinceLastSent.TotalMinutes < waitTimeMinutes)
                {
                    var secondsRemaining = (int)((waitTimeMinutes * 60) - timeSinceLastSent.TotalSeconds);

                    return BadRequest(new
                    {
                        message = $"Please wait {secondsRemaining} seconds before requesting a new code",
                        canResendIn = secondsRemaining
                    });
                }
            }

            await SendNew2FACodeAsync(user);

            _logger.LogInformation($"2FA code resent to {username}");

            return Ok(new
            {
                message = "A new verification code has been sent to your email",
                expiresIn = "5 minutes"
            });
        }

        [HttpPost("enable-2fa")]
        [Authorize]
        public async Task<IActionResult> Enable2FA()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid authentication token" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });
            if (user.TwoFactorEnabled)

                return BadRequest(new { message = "2FA already enabled" });
            await _userManager.SetTwoFactorEnabledAsync(user, true);
            _logger.LogInformation($"2FA enabled for user '{user.UserName}'");

            return Ok(new
            {
                message = "Two-factor authentication has been enabled successfully"
            });
        }

        [HttpPost("disable-2fa")]
        [Authorize]
        public async Task<IActionResult> Disable2FA()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid authentication token" });
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User cannot be found" });
            if (!user.TwoFactorEnabled)
                return BadRequest("2FA already disabled");
            await _userManager.SetTwoFactorEnabledAsync(user, false);

            // Clear any existing 2FA codes
            user.TwoFactorCode = null;
            user.TwoFactorCodeExpiry = null;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation($"2FA disabled for user '{user.UserName}'");
            return Ok(new
            {
                message = "Two-factor authentication has been disabled successfully"
            });
        }

        /// Refresh expired access token using refresh token
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            // Find refresh token in database
            var token = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token == null || !token.IsActive)
            {
                _logger.LogWarning("Invalid refresh token attempt");
                return Unauthorized(new { message = "Invalid or expired refresh token" });
            }

            var user = token.User;
            var roles = await _userManager.GetRolesAsync(user);

            // Generate new tokens
            var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
            var newRefreshToken = _tokenService.GenerateRefreshToken(GetIpAddress());

            // Mark old token as revoked
            await RevokeOldToken(token);

            // Save new refresh token
            newRefreshToken.UserId = user.Id;
            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Token refreshed for user '{user.UserName}'");

            return Ok(new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                AccessTokenExpiration = DateTime.UtcNow.AddMinutes(15),
                Username = user.UserName!,
                Email = user.Email!,
                Roles = roles.ToList()
            });
        }
        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeToken([FromBody] string refreshToken)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (token == null || !token.IsActive)
                return NotFound(new { message = "Token not found or already revoked" });
            // Mark token as revoked
            await RevokeOldToken(token);

            _logger.LogInformation("Refresh token revoked successfully");

            return Ok(new { message = "Token revoked successfully" });
        }

        // HELPER METHODS
        private async ValueTask RevokeOldToken(RefreshToken token)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = GetIpAddress();
            await _context.SaveChangesAsync();

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
        private string GenerateRandom6DigitCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private string GetIpAddress()
        {
            // Check for forwarded IP (if behind proxy/load balancer)
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"]!;

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

    }
}