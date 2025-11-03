using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Dtos.User
{
    public class UserProfileDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}