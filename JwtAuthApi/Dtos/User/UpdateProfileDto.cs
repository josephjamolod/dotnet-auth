using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Dtos.User
{
    public class UpdateProfileDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }
    }
}