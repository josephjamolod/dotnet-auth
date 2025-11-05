using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Dtos.Seller
{
    public class SellerDto
    {
        public required string Id { get; set; }
        public string? BusinessName { get; set; }
        public string? BusinessNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }

        public DateTime? RegisteredAt { get; set; } = DateTime.UtcNow;
    }
}