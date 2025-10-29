using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Data;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthApi.Services
{
    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<TokenCleanupService> _logger;

        public TokenCleanupService(
            IServiceProvider services,
            ILogger<TokenCleanupService> logger)
        {
            _services = services;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Token Cleanup Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredTokens();

                    // Wait 24 hours before next cleanup
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during token cleanup");

                    // Wait 1 hour before retrying on error
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }

            _logger.LogInformation("Token Cleanup Service stopped");
        }
        private async Task CleanupExpiredTokens()
        {
            using var scope = _services.CreateScope();

            // Get the database context from dependency injection
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

            // Delete tokens expired more than 30 days ago
            var cutoffDate = DateTime.UtcNow.AddDays(-30);

            var expiredTokens = await context.RefreshTokens
                .Where(rt => rt.ExpiresAt < cutoffDate || rt.RevokedAt < cutoffDate)
                .ToListAsync();

            if (expiredTokens.Any())
            {
                context.RefreshTokens.RemoveRange(expiredTokens);
                await context.SaveChangesAsync();

                _logger.LogInformation(
                    $"Cleaned up {expiredTokens.Count} expired refresh tokens");
            }
            else
            {
                _logger.LogInformation("No expired tokens found");
            }
        }

    }
}