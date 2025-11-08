using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Models;

namespace JwtAuthApi.Dtos.Seller
{
    public class SellerProfileDto
    {
        // Personal info
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        // Business info
        public string? BusinessName { get; set; }
        public string? BusinessNumber { get; set; }

        public string? Description { get; set; }
        public Logo? Logo { get; set; }

        // Address
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }

        //Status
        public bool IsApproved { get; set; }
        public bool IsActive { get; set; }
        public decimal Rating { get; set; }
        public int TotalRatings { get; set; }
        // Statistics
        public int TotalOrders { get; set; }
        public int TotalItems { get; set; }

        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}