using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace JwtAuthApi.Models
{
    public class AppUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        // For Two-Factor Authentication
        public string? TwoFactorCode { get; set; }
        public DateTime? TwoFactorCodeExpiry { get; set; }
        public DateTime? TwoFactorCodeLastSent { get; set; }
        public DateTime? EmailConfirmationLastSent { get; set; }

        // Navigation property for refresh tokens
        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}