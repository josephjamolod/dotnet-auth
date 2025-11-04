using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace JwtAuthApi.Models
{
    public class AppUser : IdentityUser
    {
        // Basic User Info
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // For Two-Factor Authentication
        public string? TwoFactorCode { get; set; }
        public DateTime? TwoFactorCodeExpiry { get; set; }
        public DateTime? TwoFactorCodeLastSent { get; set; }
        public DateTime? EmailConfirmationLastSent { get; set; }

        //  Seller-specific fields
        public string? BusinessName { get; set; }
        public string? BusinessNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? LogoUrl { get; set; }
        public string? Description { get; set; }

        //  Seller approval status
        public bool IsApproved { get; set; } = false;
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public string? RejectionReason { get; set; }

        //  Seller status
        public bool IsActive { get; set; } = true;
        public decimal Rating { get; set; } = 0;
        public int TotalRatings { get; set; } = 0;

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties (1 user can have many refreshtoken, fooditem,order,review)
        public List<RefreshToken> RefreshTokens { get; set; } = new();

    }
}